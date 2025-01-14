using Topic.Consumer.Services;

Task[] tasks = [
    InfoService.ReceiveInfoLog(),
    WarningService.ReceiveWarningLog(),
    ErrorService.ReceiveErrorLog(),
    ];

Task.WaitAll(tasks);