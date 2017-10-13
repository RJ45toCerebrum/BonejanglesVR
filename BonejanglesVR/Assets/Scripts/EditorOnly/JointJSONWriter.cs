using UnityEngine;
using SimpleJSON;
using com.EvolveVR.BonejanglesVR;
using System.IO;

[ExecuteInEditMode]
public class JointJSONWriter : MonoBehaviour
{
    private string file;
    public bool allowExec = true;
    void OnEnable()
    {
        file = Application.dataPath + "/Resources/BoneInfoText/" + name + ".json";
        if (File.Exists(file))
            Debug.Log(name + ": " + "Json file exisst");
        else
            Debug.Log(name + ": " + "Json file NOT exists");
            
        if (!allowExec)
            return;

        JSONNode node = null;
        CharacterJoint cj = GetComponent<CharacterJoint>();
        if(cj != null) {
            node = GetCharacterJointValues(cj);
            node[JointNode.jtypeJSON] = JointNode.JointType.Character.ToString();
            node.SaveToFile(file);
            JSONNode jn = JSONClass.LoadFromFile(file);
            Debug.Log("JType: " + jn[JointNode.jtypeJSON].ToString());
        }
        else 
        {
            HingeJoint hj = GetComponent<HingeJoint>();
            if (hj != null) 
            {
                node = GetHJValues(hj);
                node[JointNode.jtypeJSON] = JointNode.JointType.Hinge.ToString();
                node.SaveToFile(file);

                JSONNode loaded = JSONClass.LoadFromFile(file);
                Debug.Log(loaded.ToString());
            }
        }
    }

    JSONNode GetCharacterJointValues(CharacterJoint cj)
    {
        float ltl = cj.lowTwistLimit.limit;
        float htl = cj.highTwistLimit.limit;
        float s1l = cj.swing1Limit.limit;
        float s2l = cj.swing2Limit.limit;

        JSONNode node = new JSONClass();
        node[JointNode.ltlJSON] = ltl.ToString();
        node[JointNode.htlJSON] = htl.ToString();
        node[JointNode.s1lJSON] = s1l.ToString();
        node[JointNode.s2lJSON] = s2l.ToString();
       
        return node;
    }

    JSONNode GetHJValues(HingeJoint hj)
    {
        string min = hj.limits.max.ToString();
        string max = hj.limits.min.ToString();
        JSONNode jn = new JSONClass();
        jn[JointNode.minJSON] = min;
        jn[JointNode.maxJSON] = max;

        return jn;
    }

}
