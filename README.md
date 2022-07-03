# PortPolio
## 포트폴리오 페이지입니다.

유니티 2020.3.34f 버전으로 개발되었습니다.

**1. UDP - 웹캠 통신**
    
    실행방법
     - Example 씬을 엽니다.
     - 좌 상단에 목록이 있습니다. 웹캠을 선택하세요.
     - 좌측 버튼 "1. WebCam 보내기"로 데이터를 보냅니다.
     - 우측 버튼 "2. WebCam 받기"로 웹캠 데이터를 받고 화면을 구성합니다.

---
**2. ECS - Boids(군체 알고리즘)**

![GOMCAM 20220608_1004450225](https://user-images.githubusercontent.com/38842774/172509406-0c1adf16-1929-4786-8278-7ac00ee86d4b.gif)

ECS는 데이터 지향 프로그래밍으로 데이터를 병렬 처리할 수 있는 시스템입니다.

    실행방법
     - Boids 씬을 엽니다.
     - Setting의 서브 씬을 엽니다.
     - Setting에서 Boids를 설정할 수 있습니다.(미리 설정해두었습니다.)
     - 플레이합니다.

ps.개발시 주변 Entity를 검색할 때, Physics의 Overlap보다 Position을 검색하는 편이 더 빨랐습니다.

---
**3.Jobs - 섬 만들기**

![Island](https://user-images.githubusercontent.com/38842774/177042012-f7361452-f275-40c0-953f-4c5a138d46c2.png)


Job System은 안전한 멀티스레드를 제공하여 데이터를 빠르게 처리할 수 있는 시스템입니다.
노이즈를 생성(PerlinNoise, Gradient)하거나 섬 메쉬를 생성(Vertice, UV, Triangle)할 때, 데이터를 JobSystem으로 빠르게 처리하고 있습니다.

    실행방법
     - Sample 씬을 열고 플레이합니다.
     - Make 버튼을 누르면 새로운 섬을 만듭니다.

---
**4. Relection - GameObj**

Reflection은 클래스의 변수, 메소드의 정보를 가져오거나 사용하는 라이브러리입니다.

이를 이용하여 개발시 도움을 줄 수 있는 도구를 만들어보았습니다.

     - Reference 어트리뷰트
        *기능 : 정적 변수를 통해 인스턴스에 접근합니다.
        *구현 : 클래스가 Awake시 제네릭 타입 변수를 생성하고 인스턴스를 입력합니다.
        *사용방법 : Ref<T>.Ins로 접근할 수 있습니다.
        
     - SetDefault 어트리뷰트
        *기능 : 컴포넌트 변수에 기본값을 채워줍니다.
        *구현: 클래스가 Reset시 어트리뷰트를 검색하고 GetComponent로 컴포넌트를 찾아 입력합니다.
        *사용방법 : 컨텍스트 메뉴에서 SetDefault 메뉴를 호출하여 실행합니다. 
        *추가사항 : 배열, 리스트도 지원합니다.
        
     - UpdateMethod 메소드
        *기능 : Event에 등록하여 매프레임마다 실행합니다.
        *구현 : Update 메소드가 오버라이드가 되었는지 확인하고 Event에 등록합니다.
        *사용방법 : 오버라이드 함수 UpdateMethod를 사용합니다.
        
