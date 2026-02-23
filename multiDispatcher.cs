public class RvechoServer
{
    private TibrvNetTransport _transport;
    private TibrvQueue _eventQueue;
    private List<Task> _dispatchTasks = new List<Task>();
    private bool _running = false;
    private readonly int _threadCount = 4; // 멀티 디스패처 개수

    // 생성자에서 정보 수신
    public RvechoServer(string service, string network, string daemon) { /* 저장 로직 */ }

    public void Start()
    {
        _running = true;
        ConnectTransport();

        // 멀티 디스패처 실행
        for (int i = 0; i < _threadCount; i++)
        {
            _dispatchTasks.Add(Task.Run(ReceiveLoopAsync));
        }

        // Heartbeat 타이머 시작 (생략)
    }

    public void Stop()
    {
        _running = false;
        _eventQueue?.Destroy(); // 큐를 파괴하면 dispatch() 대기 중인 스레드들이 해제됨
        _transport?.Destroy();
    }

    private void ConnectTransport()
    {
        // 1. 전역 큐가 아닌 개별 큐 생성 권장 (멀티용)
        _eventQueue = new TibrvQueue();

        // 2. Transport 생성
        _transport = new TibrvNetTransport(service, network, daemon);

        // 3. 리스너 생성 및 큐 연결
        TibrvListener listener = new TibrvListener(_eventQueue, _transport, "AAAA.TG.WATCH", null);
        listener.MessageReceived += Received; 
    }

    // 기존 단일 루프를 멀티 스레드에서 동시에 실행
    private async Task ReceiveLoopAsync()
    {
        while (_running)
        {
            try
            {
                // Dispatch는 Thread-Safe하므로 여러 스레드가 동시에 호출 가능
                // 메시지가 오면 노는 스레드 중 하나가 낚아채서 Received()를 실행함
                _eventQueue.Dispatch();
            }
            catch (TibrvException ex)
            {
                // 에러 처리 및 루프 유지
            }
        }
    }

    private void Received(object sender, TibrvMsgEventArgs args)
    {
        // 1. 데이터 추출 (String 규격 유지)
        string rawData = args.Message.GetField("DATA").Value as string;

        // 2. 파싱 및 비즈니스 로직 (멀티 디스패처 환경이므로 여기서 바로 수행해도 됨)
        // System.Text.Json 역직렬화 및 에코 전송
        ProcessEcho(rawData);
    }
}
