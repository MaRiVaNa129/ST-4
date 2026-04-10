using Microsoft.VisualStudio.TestTools.UnitTesting;
using BugPro;
using Stateless;

namespace BugTests;

[TestClass]
public class BugWorkflowTests
{
    [TestMethod]
    public void Test_NewDefect_CanStartAnalysis()
    {
        var bug = new Bug();
        
        bug.StartAnalysis();
        
        Assert.AreEqual(BugState.DefectAnalysis, bug.State);
    }

    [TestMethod]
    public void Test_NewDefect_CanBeClosedDirectly()
    {
        var bug = new Bug();
        
        bug.Close();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_DefectAnalysis_CanConfirmAsDefect()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        
        bug.ConfirmAsDefect();
        
        Assert.AreEqual(BugState.Correction, bug.State);
    }

    [TestMethod]
    public void Test_DefectAnalysis_CanMarkNotDefect()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        
        bug.MarkNotDefect();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_DefectAnalysis_CanMarkDontFix()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        
        bug.MarkDontFix();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_DefectAnalysis_CanMarkDuplicate()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        
        bug.MarkDuplicate();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_DefectAnalysis_CanMarkNotReproducible()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        
        bug.MarkNotReproducible();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_Correction_CanConfirmFix()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        
        bug.ConfirmFix();
        
        Assert.AreEqual(BugState.Resolved, bug.State);
    }

    [TestMethod]
    public void Test_Correction_CanRejectFix()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        
        bug.RejectFix();
        
        Assert.AreEqual(BugState.DefectAnalysis, bug.State);
    }

    [TestMethod]
    public void Test_Resolved_CanBeClosed()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.ConfirmFix();
        
        bug.Close();
        
        Assert.AreEqual(BugState.Closure, bug.State);
    }

    [TestMethod]
    public void Test_Resolved_CanBeReopened()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.ConfirmFix();
        
        bug.Reopen();
        
        Assert.AreEqual(BugState.Reopened, bug.State);
    }

    [TestMethod]
    public void Test_Closure_CanBeClosed()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.ConfirmFix();
        bug.Close();
        
        bug.Close();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_Reopened_CanStartAnalysis()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.ConfirmFix();
        bug.Reopen();
        
        bug.StartAnalysis();
        
        Assert.AreEqual(BugState.DefectAnalysis, bug.State);
    }

    [TestMethod]
    public void Test_AssignToDeveloper_SetsAssignedPerson()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        
        bug.AssignToDeveloper("Петр Петров");
        
        Assert.AreEqual("Петр Петров", bug.AssignedTo);
    }

    [TestMethod]
    public void Test_AssignToTester_SetsAssignedPerson()
    {
        var bug = new Bug();
        
        bug.AssignToTester("Анна Сидорова");
        
        Assert.AreEqual("Анна Сидорова", bug.AssignedTo);
    }

    [TestMethod]
    public void Test_CanAssignToDeveloper_WhenInCorrectionState()
    {
        var bug = new Bug();
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        
        Assert.IsTrue(bug.CanAssignToDeveloper());
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Test_InvalidTransition_ThrowsException()
    {
        var bug = new Bug();
        
        bug.ConfirmFix();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Test_InvalidTransition_ConfirmFixFromNewDefect_ThrowsException()
    {
        var bug = new Bug();
        
        bug.ConfirmFix();
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Test_InvalidTransition_ReopenFromNewDefect_ThrowsException()
    {
        var bug = new Bug();
        
        bug.Reopen();
    }

    [TestMethod]
    public void Test_FullHappyPathWorkflow()
    {
        var bug = new Bug();
        
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.AssignToDeveloper("Иван");
        bug.ConfirmFix();
        bug.Close();
        bug.Close();
        
        Assert.AreEqual(BugState.Closed, bug.State);
    }

    [TestMethod]
    public void Test_WorkflowWithRejection()
    {
        var bug = new Bug();
        
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.RejectFix();
        
        Assert.AreEqual(BugState.DefectAnalysis, bug.State);
    }

    [TestMethod]
    public void Test_WorkflowWithReopen()
    {
        var bug = new Bug();
        
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.ConfirmFix();
        bug.Reopen();
        bug.StartAnalysis();
        
        Assert.AreEqual(BugState.DefectAnalysis, bug.State);
    }

    [TestMethod]
    public void Test_MultipleAssignments()
    {
        var bug = new Bug();
        
        bug.AssignToTester("Анна");
        bug.StartAnalysis();
        bug.ConfirmAsDefect();
        bug.AssignToDeveloper("Петр");
        
        Assert.AreEqual("Петр", bug.AssignedTo);
        Assert.AreEqual(BugState.Correction, bug.State);
    }
}
