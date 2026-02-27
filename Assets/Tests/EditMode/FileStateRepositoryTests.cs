using System;
using System.Collections.Generic;
using System.IO;
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
        var inputState = new CalculatorState
        {
            InputExpression = "54+21",
            History = new List<HistoryEntry>
            {
                HistoryEntry.Success("54+21", 75)
            }
        };

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
        var state = new CalculatorState
        {
            InputExpression = "1+2",
            History = new List<HistoryEntry> { HistoryEntry.Success("1+2", 3) }
        };
        repository.TrySave(state);

        var statePath = Path.Combine(_tempDirectory, "CalculatorState.json");
        var backupPath = statePath + ".bak";
        File.Copy(statePath, backupPath, true);
        File.WriteAllText(statePath, "{bad json");

        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to load data from"));
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

        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to load data from"));
        LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("Failed to load data from"));
        var loadedState = repository.Load();

        Assert.AreEqual(string.Empty, loadedState.InputExpression);
        Assert.NotNull(loadedState.History);
        Assert.AreEqual(0, loadedState.History.Count);
    }

    private FileStateRepository CreateRepository()
    {
        var saveLoadService = new JsonSaveLoadService(_tempDirectory);
        return new FileStateRepository(saveLoadService);
    }
}
