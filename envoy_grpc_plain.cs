var builder = WebApplication.CreateBuilder(args);

// [변경] ConfigureKestrel 내부에서 포트를 하드코딩하지 않고,
// 프로토콜(HTTP/2) 전용 설정을 환경변수로부터 유연하게 적용받도록 옵션만 제어합니다.
builder.WebHost.ConfigureKestrel(options =>
{
    // 전역 기본 프로토콜을 HTTP/2로 지정 (주소/포트는 외부 설정을 따름)
    options.Configure().Endpoint("Default", listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

builder.Services.AddGrpc();
var app = builder.Build();

// [핵심 마스터 키] Envoy의 :scheme https와 실제 http 간의 미스매치 원천 차단
app.Use((context, next) =>
{
    // 1. Envoy 가 넘겨준 원본 정보 및 프로토콜 확인
    var originalScheme = context.Request.Scheme;
    var protocol = context.Request.Protocol; // 예: HTTP/2
    
    // 2. 가상 헤더 세부 추적용 (디버깅 시 유용)
    var xForwardedProto = context.Request.Headers["X-Forwarded-Proto"].ToString();

    // [로깅 포인트] 원본 scheme이 http가 아니거나, 디버깅 모드일 때 기록
    if (originalScheme != "http")
    {
        // 실무에서는 대량의 gRPC 요청이 들어오므로 구체적인 Method 경로도 함께 찍어주면 좋습니다.
        var path = context.Request.Path; 
        
        Console.WriteLine($"[gRPC Scheme Fix] Modified Scheme: {originalScheme} -> http | Protocol: {protocol} | X-Forwarded-Proto: {xForwardedProto} | Path: {path}");
        
        // 만약 ILogger를 사용한다면 아래 구조를 권장합니다.
        // logger.LogInformation("gRPC Scheme bypassed. Original: {OriginalScheme}, X-Proto: {XProto}, Path: {Path}", originalScheme, xForwardedProto, path);
    }

    // 3. 실제 변조 수행
    context.Request.Scheme = "http";
    
    return next();
});

app.MapGrpcService<YourGrpcService>();
app.Run();
