```
# 토큰 추출
SA_TOKEN=$(kubectl get secret kubesphere-token -n kubesphere-system -o jsonpath='{.data.token}' | base64 --decode)

# CA 인증서 추출
CA_CRT=$(kubectl get secret kubesphere-token -n kubesphere-system -o jsonpath='{.data.ca\.crt}')
```


```
apiVersion: v1
kind: Config
clusters:
- cluster:
    certificate-authority-data: <대입: $CA_CRT 값 전체>
    server: https://kubernetes.default.svc:443
  name: local-cluster
contexts:
- context:
    cluster: local-cluster
    user: kubesphere-user
    namespace: kubesphere-system
  name: kubesphere-context
current-context: kubesphere-context
users:
- name: kubesphere-user
  user:
    token: <대입: $SA_TOKEN 값 전체>
```
