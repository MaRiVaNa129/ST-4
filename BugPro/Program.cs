using Stateless;

namespace BugPro;

public enum BugState
{
    NewDefect,          
    DefectAnalysis,     
    Correction,         
    Closure,            
    Reopened,           
    Resolved,           
    Closed              
}

public enum BugTrigger
{
    StartAnalysis,          
    
    ConfirmAsDefect,        
    NotDefect,              
    DontFix,                
    Duplicate,              
    NotReproducible,        
    
    ConfirmFix,             
    RejectFix,              
    
    Close,                  
    Reopen,                 
    
    AssignToTester,         
    AssignToProductTeam,    
    AssignToDeveloper       
}

public class Bug
{
    private readonly StateMachine<BugState, BugTrigger> _machine;
    private string _assignedTo = string.Empty;
    
    private readonly StateMachine<BugState, BugTrigger>.TriggerWithParameters<string> _assignToTesterParam;
    private readonly StateMachine<BugState, BugTrigger>.TriggerWithParameters<string> _assignToProductTeamParam;
    private readonly StateMachine<BugState, BugTrigger>.TriggerWithParameters<string> _assignToDeveloperParam;

    public BugState State => _machine.State;
    public string AssignedTo => _assignedTo;

    public Bug()
    {
        _machine = new StateMachine<BugState, BugTrigger>(BugState.NewDefect);
        
        _assignToTesterParam = _machine.SetTriggerParameters<string>(BugTrigger.AssignToTester);
        _assignToProductTeamParam = _machine.SetTriggerParameters<string>(BugTrigger.AssignToProductTeam);
        _assignToDeveloperParam = _machine.SetTriggerParameters<string>(BugTrigger.AssignToDeveloper);

        ConfigureTransitions();
    }

    private void ConfigureTransitions()
    {
        _machine.Configure(BugState.NewDefect)
            .Permit(BugTrigger.StartAnalysis, BugState.DefectAnalysis)
            .Permit(BugTrigger.Close, BugState.Closed)
            .OnEntry(() => Console.WriteLine("[НОВЫЙ ДЕФЕКТ] Баг зарегистрирован"));

        _machine.Configure(BugState.DefectAnalysis)
            .Permit(BugTrigger.ConfirmAsDefect, BugState.Correction)
            .Permit(BugTrigger.NotDefect, BugState.Closed)
            .Permit(BugTrigger.DontFix, BugState.Closed)
            .Permit(BugTrigger.Duplicate, BugState.Closed)
            .Permit(BugTrigger.NotReproducible, BugState.Closed)
            .OnEntry(() => Console.WriteLine("[РАЗБОР ДЕФЕКТОВ] Анализ бага"));

        _machine.Configure(BugState.Correction)
            .Permit(BugTrigger.ConfirmFix, BugState.Resolved)
            .Permit(BugTrigger.RejectFix, BugState.DefectAnalysis)
            .Permit(BugTrigger.AssignToDeveloper, BugState.Correction)
            .OnEntry(() => Console.WriteLine("[ИСПРАВЛЕНИЕ] Разработка исправления"));

        _machine.Configure(BugState.Resolved)
            .Permit(BugTrigger.Close, BugState.Closure)
            .Permit(BugTrigger.Reopen, BugState.Reopened)
            .OnEntry(() => Console.WriteLine("[ПРОБЛЕМА РЕШЕНА?] Ожидание подтверждения"));

        _machine.Configure(BugState.Closure)
            .Permit(BugTrigger.Close, BugState.Closed)
            .OnEntry(() => Console.WriteLine("[ЗАКРЫТИЕ] Подготовка к закрытию"));

        _machine.Configure(BugState.Reopened)
            .Permit(BugTrigger.StartAnalysis, BugState.DefectAnalysis)
            .OnEntry(() => Console.WriteLine("[ПЕРЕОТКРЫТИЕ] Баг требует доработки"));

        _machine.Configure(BugState.Closed)
            .OnEntry(() => Console.WriteLine("[ЗАКРЫТ] Баг закрыт"));
    }

    // Методы для внешнего вызова
    
    public void StartAnalysis() => _machine.Fire(BugTrigger.StartAnalysis);
    public void ConfirmAsDefect() => _machine.Fire(BugTrigger.ConfirmAsDefect);
    public void MarkNotDefect() => _machine.Fire(BugTrigger.NotDefect);
    public void MarkDontFix() => _machine.Fire(BugTrigger.DontFix);
    public void MarkDuplicate() => _machine.Fire(BugTrigger.Duplicate);
    public void MarkNotReproducible() => _machine.Fire(BugTrigger.NotReproducible);
    public void ConfirmFix() => _machine.Fire(BugTrigger.ConfirmFix);
    public void RejectFix() => _machine.Fire(BugTrigger.RejectFix);
    public void Close() => _machine.Fire(BugTrigger.Close);
    public void Reopen() => _machine.Fire(BugTrigger.Reopen);
    
    public void AssignToTester(string person)
    {
        _assignedTo = person;
        _machine.Fire(_assignToTesterParam, person);
        Console.WriteLine($"Баг назначен тестировщику: {person}");
    }
    
    public void AssignToProductTeam(string person)
    {
        _assignedTo = person;
        _machine.Fire(_assignToProductTeamParam, person);
        Console.WriteLine($"Баг назначен продуктовой команде: {person}");
    }
    
    public void AssignToDeveloper(string person)
    {
        _assignedTo = person;
        _machine.Fire(_assignToDeveloperParam, person);
        Console.WriteLine($"Баг назначен разработчику: {person}");
    }
    
    public bool CanAssignToTester() => _machine.CanFire(BugTrigger.AssignToTester);
    public bool CanAssignToDeveloper() => _machine.CanFire(BugTrigger.AssignToDeveloper);
}

class Program
{
    static void Main()
    {
        Console.WriteLine("=== Демонстрация WorkFlow бага ===\n");
        
        var bug = new Bug();
        
        Console.WriteLine($"Текущее состояние: {bug.State}\n");
        
        // Полный цикл жизни бага
        bug.StartAnalysis();
        Console.WriteLine($"-> Состояние: {bug.State}\n");
        
        bug.ConfirmAsDefect();
        Console.WriteLine($"-> Состояние: {bug.State}\n");
        
        bug.AssignToDeveloper("Иван Иванов");
        bug.ConfirmFix();
        Console.WriteLine($"-> Состояние: {bug.State}\n");
        
        bug.Close();
        Console.WriteLine($"-> Состояние: {bug.State}\n");
        
        Console.WriteLine("\n=== Демонстрация отклонения исправления ===");
        var bug2 = new Bug();
        bug2.StartAnalysis();
        bug2.ConfirmAsDefect();
        bug2.RejectFix(); // Исправление отклонено
        Console.WriteLine($"-> Состояние: {bug2.State} (возврат к анализу)\n");
        
        Console.WriteLine("\n=== Демонстрация переоткрытия ===");
        var bug3 = new Bug();
        bug3.StartAnalysis();
        bug3.ConfirmAsDefect();
        bug3.ConfirmFix();
        bug3.Reopen(); // Баг переоткрыт
        Console.WriteLine($"-> Состояние: {bug3.State}\n");
        
        Console.WriteLine("\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}
