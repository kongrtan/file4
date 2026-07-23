app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        // 200 OK 포함 정상 응답(2xx)은 Information 대신 Debug 레벨로 로그를 출력
        if (httpContext.Response.StatusCode >= 200 && httpContext.Response.StatusCode < 300)
        {
            return LogEventLevel.Debug;
        }

        // 500 이상은 Error 레벨
        if (httpContext.Response.StatusCode >= 500)
        {
            return LogEventLevel.Error;
        }

        return LogEventLevel.Information;
    };
});
