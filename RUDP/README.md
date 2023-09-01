RUDP는 신뢰성을 갖는 UDP를 의미합니다.
 - TCP는 신뢰할 수 있지만 느립니다.  
 - UDP는 신뢰할 수 없고 빠릅니다.  
UDP를 통해 TCP의 신뢰성을 갖춘다면 더욱 빠르게 통신할 수 있습니다.
<br/><br/><br/>

[RUDP 구성 설명]
 - 데이터 순서 확인
 - 데이터 도착 확인
 - 데이터 무결성 확인
<br/><br/><br/>

[주요 클래스]  
BaseRUDP : 일반 byte배열을 보내는 소켓 통신.  
RUDP : BaseRUDP로 DataGram을 Serialize하여 통신함.  
Buffer : byte배열을 쓰거나 읽는 용도.  
Packet : Buffer에 Header를 쓰거나 읽는 용도.  
Datagram : Buffer에 실제 데이터를 쓰기 위한 용도.  
<br/><br/><br/>

[현재 데이터 구조]  
Packet
 - Header : 8byte
    - Reliable : 1bit
    - Sender : 1bit
    - Success : 1bit
    - Index : 1byte
    - CRC : 2byte
    - Length : 4byte
 - Buffer
    - Datagram : 1015byte
        - DataType : 1byte
        - Bytes : 1014byte
<br/><br/><br/>

[완료된 작업]
 - 네트워크 연결
 - 데이터 통신
 - 데이터 순서 확인
 - 데이터 무결성 확인
 - 데이터 도착 확인
<br/><br/><br/>
  
[남은 작업]  
 - TimeOut
 - Texture 통신
 - 예제 씬 구성
