using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SceneManager : MonoBehaviour
{
    #region Unity Inspector Fields
    public Transform PocketProxyPrefab = null;
    public Transform PocketParent = null;
    #endregion

    #region Constants
    public const float UNIFORM_SIZE = 10f;
	public const string KeyURL = "ServerURL";
	#endregion

	#region Fields
	protected string _MultiverseURL = "";
	#endregion

	#region Properties
	public bool Started
	{
		get
		{
			return (Created != null);
		}
	}

	public string MultiverseURL
	{
		get
		{
			return _MultiverseURL;
		}
		set
		{
			_MultiverseURL = value ?? "";
			if (MultiverseURL != "" && Started)
			{
				StartCoroutine(CheckWeb(true));
			}
		}
	}

	protected string InputURL { get; set; }

    protected bool Refreshing { get; set; }

    protected float LastCheckTime { get; set; }

    protected Dictionary<int, Transform> Created { get; set; }

    protected bool UniformSize { get; set; }
    #endregion

    #region Unity Events
    void Start()
    {
		string TempURL = PlayerPrefs.GetString(KeyURL, null);
		if (!string.IsNullOrEmpty(TempURL))
		{
			MultiverseURL = TempURL;
		}

        Created = new Dictionary<int, Transform>();
        StartCoroutine(CheckWeb(true));
    }

    void Update()
    {
        StartCoroutine(CheckWeb());
    }

    void OnGUI()
    {
		if (string.IsNullOrEmpty(MultiverseURL))
		{
			GUI.Box(new Rect(10, 10, Screen.width - 10, Screen.height - 10), "");
			if (InputURL == null) { InputURL = ""; }
			InputURL = GUI.TextField(new Rect((Screen.width / 2) - 100, (Screen.height / 2) - 15, 200, 30), InputURL);
			if (GUI.Button(new Rect((Screen.width / 2) - 100, (Screen.height / 2) + 20, 200, 30), "Save"))
			{
				if (!InputURL.StartsWith("ht")) { InputURL = "http://" + InputURL; }
				if (!InputURL.EndsWith("/")) { InputURL += "/"; }

				MultiverseURL = InputURL;
				PlayerPrefs.SetString(KeyURL, MultiverseURL);
			}
		}
		else
		{
			if (GUI.Button(new Rect(10, 10, 100, 30), "Toggle Size"))
			{
				UniformSize = !UniformSize;

				foreach (Transform OnePocket in Created.Values)
				{
					OnePocket.GetComponent<PocketControl>().TargetScale = UniformSize ? UNIFORM_SIZE : OnePocket.GetComponent<PocketControl>().Size;
				}
			}
		}
    }
    #endregion

    #region Methods
    IEnumerator CheckWeb(bool forceRefresh = false)
    {
        if (Refreshing || MultiverseURL == "") { yield break; }

        if ((Time.time - LastCheckTime) >= 60 || forceRefresh)
        {
            Refreshing = true;
            Debug.Log("Grab Fom Web: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            LastCheckTime = Time.time;

			// Check Web
			WWW www = new WWW(MultiverseURL + "/getMock");
            yield return www;
            if (www.error == null)
            {
                //Debug.Log(www.text);
                Dictionary<string, object> Data = fastJSON.JSON.Parse(www.text) as Dictionary<string, object>;
                if (Data != null) { UpdatePositions(Data); }
                else { Debug.Log("No data!"); }
            }
            else
			{
				Debug.Log(www.error);
				MultiverseURL = null;
			}

            Refreshing = false;
        }
    }

    protected void UpdatePositions(Dictionary<string, object> Data)
    {
        List<object> Universes = Data["Universes"] as List<object>;

        Dictionary<string, object> U;
        int TempId;
        string TempName;
        float TempFloat;
        float TempSize;
        float X;
        float Y;
        float Z;
        Vector3 TempPos;

        List<int> CollapsedPockets = new List<int>(Created.Keys);

        foreach (object Universe in Universes)
        {
            U = Universe as Dictionary<string, object>;
            if (U == null) { continue; }

            TempId = (int)((System.Int64)U["UniverseId"]);
            TempName = (string)U["Name"];
            CollapsedPockets.Remove(TempId);
            TempSize = float.Parse(U["Size"].ToString());
            string[] Parts = ((string)U["Position"]).Split(new char[] { ',' });
            if (Parts.Length == 3)
            {
                if (float.TryParse(Parts[0], out TempFloat)) { X = TempFloat; } else { X = 0; }
                if (float.TryParse(Parts[1], out TempFloat)) { Y = TempFloat; } else { Y = 0; }
                if (float.TryParse(Parts[2], out TempFloat)) { Z = TempFloat; } else { Z = 0; }
                TempPos = new Vector3(X, Y, Z);
            }
            else
            {
                TempPos = new Vector3();
            }

            if (Created.ContainsKey(TempId))
            {
                Created[TempId].position = TempPos;
                TempFloat = TempSize / 10f;
                Created[TempId].GetComponent<PocketControl>().Size = TempFloat;
                if (!UniformSize) { Created[TempId].GetComponent<PocketControl>().TargetScale = TempFloat; }
            }
            else
            {
                Transform NewPocket = Instantiate(PocketProxyPrefab, TempPos, Quaternion.identity) as Transform;
                NewPocket.name = TempName;
                NewPocket.parent = PocketParent;
                TempFloat = TempSize / 10f;
                NewPocket.GetComponent<PocketControl>().Size = TempFloat;
                if (!UniformSize) { NewPocket.GetComponent<PocketControl>().TargetScale = TempFloat; }
                else { NewPocket.GetComponent<PocketControl>().TargetScale = UNIFORM_SIZE; }
                Created.Add(TempId, NewPocket);
            }
        }

        foreach (int OneId in CollapsedPockets)
        {
            Created[OneId].GetComponent<PocketControl>().TargetScale = 0f;
            Created.Remove(OneId);
        }
    }
    #endregion
}
