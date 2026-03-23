using System.Collections.Generic;

[System.Serializable]
public class TopicData
{
    public string topicName;
    public List<GroupQuestionData> groups = new List<GroupQuestionData>();
}

[System.Serializable]
public class GroupQuestionData
{
    public string groupName;
    public List<string> words = new List<string>();
    public List<string> questions = new List<string>();
    public int levelCount = 3;
}
