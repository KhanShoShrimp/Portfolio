![사진](https://github.com/KhanShoShrimp/PortPolio/assets/38842774/348962f9-4147-4957-ac00-23e567c52cf0)
RUDP는 신뢰성을 갖는 UDP를 의미합니다.<br/>
[RUDP 구성 설명]
 - 데이터 순서 확인
 - 데이터 도착 확인
 - 데이터 무결성 확인
<br/>
저는 RUDP 사용예시로 WebCam을 통신해보았습니다.

[주요 클래스]  
BaseRUDP : 일반 byte배열을 보내는 소켓 통신.  
RUDP : BaseRUDP로 DataGram을 Serialize하여 통신함.  
Buffer : byte배열을 쓰거나 읽는 용도.  
Packet : Buffer에 Header를 쓰거나 읽는 용도.  
Datagram : Buffer에 실제 데이터를 쓰기 위한 용도.  
<br/><br/><br/>

[데이터 구조]  
- Packet : 1024bytes
  - Header : 8bytes
    - Reliable : 1bit
    - Sender : 1bit
    - Success : 1bit
    - Index : 1byte
    - CRC : 2bytes
    - Length : 4bytes
  - Buffer
    - Datagram : 1015bytes
        - DataType : 1byte
        - Bytes : 1014bytes
<br/><br/><br/>

[작업 내역]  
 - 네트워크 연결
 - 데이터 통신
 - 데이터 순서 확인
 - 데이터 무결성 확인
 - 데이터 도착 확인
 - TimeOut
<br/><br/><br/>
