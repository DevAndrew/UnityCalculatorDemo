using System;
using System.IO;
using System.Text.RegularExpressions;
using DevAndrew.SaveLoad.Infrastructure;
using DevAndrew.Calculator.Infrastructure;
using DevAndrew.Calculator.Core.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FileStateRepositoryTests
{
    private string _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "CalculatorDemoTaskTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public void SaveAndLoad_RoundTrip_Works()
    {
        var repository = CreateRepository();
        var inputState = CalculatorState.CreateDefault();
        inputState.TrySetInputExpression("54+21");
        inputState.AddSuccessHistory("54+21", 75);

        var saveResult = repository.TrySave(inputState);
        var loadedState = repository.Load();

        Assert.IsTrue(saveResult);
        Assert.AreEqual("54+21", loadedState.InputExpression);
        Assert.AreEqual(1, loadedState.History.Count);
        Assert.AreEqual("54+21", loadedState.History[0].Expression);
        Assert.IsFalse(loadedState.History[0].IsError);
        Assert.AreEqual(75, loadedState.History[0].Result);
    }

    [Test]
    public void Load_UsesBackup_WhenPrimaryFileIsCorrupted()
    {
        var repository = CreateRepository();
        var state = CalculatorState.CreateDefault();
        state.TrySetInputExpression("1+2");
        state.AddSuccessHistory("1+2", 3);
        repository.TrySave(state);

        var statePath = Path.Combine(_tempDirectory, "CalculatorState.json");
        var backupPath = statePath + ".bak";
        File.Copy(statePath, backupPath, true);
        File.WriteAllText(statePath, "{bad json");

        LogAssert.Expect(LogType.Error, new Regex("Failed to load data from"));
        var loadedState = repository.Load();

        Assert.AreEqual("1+2", loadedState.InputExpression);
        Assert.AreEqual(1, loadedState.History.Count);
        Assert.AreEqual("1+2", loadedState.History[0].Expression);
        Assert.IsFalse(loadedState.History[0].IsError);
        Assert.AreEqual(3, loadedState.History[0].Result);
    }

    [Test]
    public void Load_ReturnsDefault_WhenBothPrimaryAndBackupAreCorrupted()
    {
        var repository = CreateRepository();

        var statePath = Path.Combine(_tempDirectory, "CalculatorState.json");
        var backupPath = statePath + ".bak";
        File.WriteAllText(statePath, "{bad json");
        File.WriteAllText(backupPath, "{bad json too");

        LogAssert.Expect(LogType.Error, new Regex("Failed to load data from"));
        LogAssert.Expect(LogType.Error, new Regex("Failed to load data from"));
        var loadedState = repository.Load();

        Assert.AreEqual(string.Empty, loadedState.InputExpression);
        Assert.NotNull(loadedState.History);
        Assert.AreEqual(0, loadedState.History.Count);
    }

    [Test]
    public void SaveAndLoad_RespectsHistoryCap()
    {
        var repository = CreateRepository();
        var state = CalculatorState.CreateDefault();
        var totalEntries = CalculatorState.MaxHistoryEntries + 50;
        for (var i = 0; i < totalEntries; i++)
        {
            state.AddSuccessHistory($"{i}+0", i);
        }

        repository.TrySave(state);
        var loadedState = repository.Load();

        Assert.AreEqual(CalculatorState.MaxHistoryEntries, loadedState.History.Count);
        Assert.AreEqual("50+0", loadedState.History[0].Expression);
        Assert.AreEqual("1049+0", loadedState.History[loadedState.History.Count - 1].Expression);
    }

    [Test]
    public void Save_AfterLoadWithNullHistory_DoesNotThrow_AndWritesHistory()
    {
        var repository = CreateRepository();
        var brokenJson = "{\"version\":1,\"inputExpression\":\"2+2\",\"history\":null}";
        File.WriteAllText(Path.Combine(_tempDirectory, "CalculatorState.json"), brokenJson);

        var loaded = repository.Load();
        loaded.AddSuccessHistory("2+2", 4);

        var saveResult = repository.TrySave(loaded);
        var reloaded = CreateRepository().Load();

        Assert.IsTrue(saveResult);
        Assert.AreEqual(1, reloaded.History.Count);
        Assert.AreEqual("2+2", reloaded.History[0].Expression);
        Assert.AreEqual(4, reloaded.History[0].Result);
    }

    [Test]
    public void Save_WithUnchangedState_DoesNotDuplicateHistory()
    {
        var repository = CreateRepository();
        var state = CalculatorState.CreateDefault();
        state.TrySetInputExpression("9+1");
        state.AddSuccessHistory("9+1", 10);
        repository.TrySave(state);

        var secondSave = repository.TrySave(state);
        var loaded = CreateRepository().Load();

        Assert.IsTrue(secondSave);
        Assert.AreEqual(1, loaded.History.Count);
        Assert.AreEqual("9+1", loaded.History[0].Expression);
    }

    [Test]
    public void Save_WhenHistoryGrows_AppendsWithoutRebuildingSemantics()
    {
        var repository = CreateRepository();
        var state = CalculatorState.CreateDefault();
        state.AddSuccessHistory("1+1", 2);
        repository.TrySave(state);

        state.AddSuccessHistory("2+2", 4);
        var secondSave = repository.TrySave(state);
        var loaded = CreateRepository().Load();

        Assert.IsTrue(secondSave);
        Assert.AreEqual(2, loaded.History.Count);
        Assert.AreEqual("1+1", loaded.History[0].Expression);
        Assert.AreEqual("2+2", loaded.History[1].Expression);
    }

    [Test]
    public void Save_WhenHistorySlidesAtCap_KeepsCorrectWindow()
    {
        var repository = CreateRepository();
        var state = CalculatorState.CreateDefault();

        for (var i = 0; i < CalculatorState.MaxHistoryEntries; i++)
        {
            state.AddSuccessHistory($"{i}+0", i);
        }

        repository.TrySave(state);

        state.AddSuccessHistory($"{CalculatorState.MaxHistoryEntries}+0", CalculatorState.MaxHistoryEntries);
        var secondSave = repository.TrySave(state);
        var loaded = CreateRepository().Load();

        Assert.IsTrue(secondSave);
        Assert.AreEqual(CalculatorState.MaxHistoryEntries, loaded.History.Count);
        Assert.AreEqual("1+0", loaded.History[0].Expression);
        Assert.AreEqual($"{CalculatorState.MaxHistoryEntries}+0", loaded.History[loaded.History.Count - 1].Expression);
    }

    private FileStateRepository CreateRepository()
    {
        var saveLoadService = new JsonSaveLoadService(_tempDirectory);
        return new FileStateRepository(saveLoadService);
    }
}
