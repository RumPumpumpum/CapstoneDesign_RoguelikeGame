using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
// Start is called before the first frame update
{
    /// <summary>
    /// 확인할 곳의 문자열이 비어있는지 확인, 비어있으면 "문자열을 입력해야 합니다" 경고
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if(stringToCheck == "") 
        {
            Debug.Log(fieldName + " 항목이 비어있습니다. 문자열을 입력해야 합니다. 해당 오브젝트: " + thisObject.name.ToString());
            return true;

        }
        return false;
    }

    /// <summary>
    /// 리스트에 null값이 있거나 목록이 비어있을 경우 true 반환
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if(enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + " 항목이 비어있습니다. 해당 오브젝트: " + thisObject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null) // 목록에 null 값 항목이 있는지 확인
            {
                Debug.Log(fieldName + " 가 null 값을 가지고 있습니다. 해당 오브젝트: " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0) // count가 0인지 검사, count가 0이다 => 열거 가능한 객체에 값이 없다.
            {
                Debug.Log(fieldName + " 목록이 비어있습니다 . 해당 오브젝트: " + thisObject.name.ToString());
                error = true;
            }
            return error;
        
    }
}
