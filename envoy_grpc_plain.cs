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
    context.Request.Scheme = "http";
    return next();
});

app.MapGrpcService<YourGrpcService>();
app.Run();
