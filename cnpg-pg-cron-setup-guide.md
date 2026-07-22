# CloudNativePG (CNPG) PostgreSQL pg_cron 설정 가이드

본 가이드는 CloudNativePG(CNPG) 환경에서 `pg_cron` 확장을 설치하고 설정하는 가장 간단한 방법을 정리한 문서입니다.

---

## 1. 개요 및 사전 지식

* **공식 이미지 패키지 포함 여부**: CNPG 공식 커뮤니티 이미지(`ghcr.io/cloudnative-pg/postgresql`)에는 `pg_cron`, `pg_stat_statements`, `pgaudit` 등 자주 사용되는 주요 확장 패키지가 이미 빌드 시점에 포함되어 있습니다.
* **필수 요구사항**: `pg_cron`은 백그라운드 프로세스로 동작하므로, PostgreSQL 실행 시 `shared_preload_libraries`에 등록되어야 합니다.

---

## 2. CNPG Cluster 매니페스트 (YAML) 설정

`Cluster` 매니페스트의 `postgresql.parameters` 항목에 `shared_preload_libraries`와 `cron.database_name`을 추가합니다.

```yaml
apiVersion: postgresql.cnpg.io/v1
kind: Cluster
metadata:
  name: postgres-cluster
spec:
  instances: 3

  # 사용 중인 PostgreSQL 이미지 버전 지정
  imageName: ghcr.io/cloudnative-pg/postgresql:18.3

  postgresql:
    parameters:
      # 1. pg_cron 백그라운드 프로세스 로드
      shared_preload_libraries: "pg_cron"
      # 2. pg_cron 메타데이터 및 스케줄 테이블이 저장될 DB 지정 (기본값: postgres)
      cron.database_name: "postgres"

  storage:
    size: 10Gi
```

> **주의**: `shared_preload_libraries` 파라미터가 변경되면 Pod 재시작(Rolling Update)이 진행됩니다.

---

## 3. PostgreSQL Extension 활성화 및 권한 설정

배포 완료 후, `cron.database_name`으로 지정한 데이터베이스(`postgres`)에 접속하여 확장을 생성합니다.

```sql
-- 1. DB 접속 (cron.database_name으로 지정한 DB)
\c postgres

-- 2. Extension 활성화
CREATE EXTENSION pg_cron;

-- 3. (선택) 일반 사용자에게 스키마 및 테이블 권한 부여
GRANT USAGE ON SCHEMA cron TO app_user;
```

---

## 4. pg_cron 사용법 및 주요 명령

### (1) 스케줄 작업 등록
`cron.schedule` 함수를 이용하여 Cron 표현식 기반 작업 생성:

```sql
-- 예시 1: 매분마다 VACUUM ANALYZE 실행
SELECT cron.schedule('vacuum-job', '* * * * *', 'VACUUM ANALYZE;');

-- 예시 2: 매일 새벽 3시에 특정 테이블 정리
SELECT cron.schedule('daily-cleanup', '0 3 * * *', 'DELETE FROM audit_logs WHERE created_at < NOW() - INTERVAL ''30 days'';');
```

### (2) 등록된 작업 목록 조회
```sql
SELECT jobid, schedule, command, nodename, nodeport, database, username, active, jobname 
FROM cron.job;
```

### (3) 작업 실행 이력 조회
```sql
SELECT jobid, runid, job_pid, database, username, command, status, return_message, start_time, end_time 
FROM cron.job_run_details 
ORDER BY start_time DESC 
LIMIT 20;
```

### (4) 작업 삭제 및 비활성화
```sql
-- 작업 이름으로 삭제
SELECT cron.unschedule('vacuum-job');

-- 작업 ID로 삭제
SELECT cron.unschedule(1);
```
