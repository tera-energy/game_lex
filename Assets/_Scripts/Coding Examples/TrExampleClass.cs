using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrExampleClass : MonoBehaviour
{
    const float _ABC_DEF = 1f;
    static int _AbcDef;
    int _abcDef;
    [SerializeField] string _qweXyz;
    Vector3 _origPos; // 리셋(게임오브젝트가 처음 위치로 돌아갈 때 등)할 때 필요한 변수는 앞에 orig(original)을 붙여주세요. 
    Coroutine _procCoroutine1;
    Coroutine _procCoroutine2;
    
    public SpriteRenderer _image; // 단순 레퍼런스로만 사용할 시 public 사용가능.
    [SerializeField] Transform _transform; // this 게임오브젝트의 Transform을 캐시하여 직접 변경을 가할 변수이므로 private.
    
    TT.CallbackBool _onAbcDef;        // Delegate이나 이벤트 같은 변수들은 on을 앞에 넣어주세요.
    public event TT.CallbackInt _onAbcXyz;
    TrAbc_Def _trAbc_Def;

    // 메소드 prefix ===> 프로퍼티:xAbcDef, 퍼블릭&프로텍트:zAbcDef, 프라이빗:yAbcDef
    public int xAbcDef { get { return _AbcDef; } set { _AbcDef = value; } }
    public string xQweXyz { get { return _qweXyz; } }

    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // 유니티 함수
    void Start() {
        _onAbcDef += yOnBool;
        _onAbcXyz += yOnRecieveResponse;
        _onAbcDef.Invoke(true);
        _onAbcXyz.Invoke(0);

        _procCoroutine1 = StartCoroutine(yProcCoroutine1(0));
        _procCoroutine2 = StartCoroutine(yProcCoroutine2(0));
    }


    void Update() {
        int abcDef = 0;
        abcDef++;
    }


    //=====================================================================================================
    // 클래스 함수
    public void zFooMethod(string abcDef) {
        _trAbc_Def.zMethod1();
        _trAbc_Def.zzMethod2();
    }

    void yMooMethod(int abcDef) {
        StopCoroutine(_procCoroutine1);
        StopCoroutine(_procCoroutine2);
    }

    void yOnBool(bool isYes)    // 이벤트 핸들러엔 메소드prefix 뒤에 On을 붙여주세요.
    {
        if(isYes) {

        } else {

        }
    }

    void yOnRecieveResponse(int code) {
        if(code == 0) {

        } else if(code == 1) {

        } else if(code == 2) {

        } else {

        }
    }

    IEnumerator yProcCoroutine1(int x)  // 코루틴엔 Proc을...
    {
        yield return null;
    }

    IEnumerator yProcCoroutine2(int y) {
        yield return null;
    }
}
