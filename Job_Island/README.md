# PortPolio
## 포트폴리오 페이지입니다.
유니티 2020.3.34f 버전으로 개발되었습니다.

Jobs - 섬 만들기

![Island](https://user-images.githubusercontent.com/38842774/177042012-f7361452-f275-40c0-953f-4c5a138d46c2.png)


Job System은 안전한 멀티스레드를 제공하여 데이터를 빠르게 처리할 수 있는 시스템입니다.

노이즈를 생성(PerlinNoise, Gradient)하거나 섬 메쉬를 생성(Vertice, UV, Triangle)할 때, 데이터를 JobSystem으로 처리하고 있습니다.

개발 내역
- Delaunay triangulation
- Fortune's sweep(작업 취소)
- Lloyd's algorithm
