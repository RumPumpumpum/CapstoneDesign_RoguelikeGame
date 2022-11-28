using UnityEngine;

// 싱글톤의 버전이 하나만 있는지 확인, 중복버전이 있으면 파괴한다.

public abstract class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour // monobehaviour의 하위 클래스만 T로 사용 가능
{
    private static T instance;

    public static T Instance
    {
        get
        {
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if(instance == null)
        {
            instance = this as T;
        }

        else
        {
            Destroy(gameObject);
        }
    }

}
