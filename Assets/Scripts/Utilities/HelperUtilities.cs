using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HelperUtilities
// Start is called before the first frame update
{
    /// <summary>
    /// Ȯ���� ���� ���ڿ��� ����ִ��� Ȯ��, ��������� "���ڿ��� �Է��ؾ� �մϴ�" ���
    /// </summary>
    public static bool ValidateCheckEmptyString(Object thisObject, string fieldName, string stringToCheck)
    {
        if(stringToCheck == "") 
        {
            Debug.Log(fieldName + " �׸��� ����ֽ��ϴ�. ���ڿ��� �Է��ؾ� �մϴ�. �ش� ������Ʈ: " + thisObject.name.ToString());
            return true;

        }
        return false;
    }

    /// <summary>
    /// ����Ʈ�� null���� �ְų� ����� ������� ��� true ��ȯ
    /// </summary>
    public static bool ValidateCheckEnumerableValues(Object thisObject, string fieldName, IEnumerable enumerableObjectToCheck)
    {
        bool error = false;
        int count = 0;

        if(enumerableObjectToCheck == null)
        {
            Debug.Log(fieldName + " �׸��� ����ֽ��ϴ�. �ش� ������Ʈ: " + thisObject.name.ToString());
            return true;
        }

        foreach (var item in enumerableObjectToCheck)
        {
            if (item == null) // ��Ͽ� null �� �׸��� �ִ��� Ȯ��
            {
                Debug.Log(fieldName + " �� null ���� ������ �ֽ��ϴ�. �ش� ������Ʈ: " + thisObject.name.ToString());
                error = true;
            }
            else
            {
                count++;
            }
        }

        if (count == 0) // count�� 0���� �˻�, count�� 0�̴� => ���� ������ ��ü�� ���� ����.
            {
                Debug.Log(fieldName + " ����� ����ֽ��ϴ� . �ش� ������Ʈ: " + thisObject.name.ToString());
                error = true;
            }
            return error;
        
    }
}
