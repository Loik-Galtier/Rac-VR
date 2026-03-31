using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using TMPro;
using UnityEngine.SceneManagement;


public class GlobalTest : MonoBehaviour
{
    /*
     public GameObject Player;
     public GameObject PNJ1;
     public GameObject Ground;
     public GameObject WasteDisplayM;
     public GameObject WasteCollectionI;
     public GameObject ModeConfigM;
     public GameObject HelpM;

     public List<GameObject> Agents;

     //optional: rotation, Y-translation and Size scale to apply to the prefabs correspoding to the different species of agents
     public List<float> rotations = new List<float> { 90.0f, 90.0f, 0.0f };
     public List<float> rotationsCoeff = new List<float> { 1, 1, 0.0f };
     public List<float> YValues = new List<float> { -0.9f, -0.9f, 0.15f };
     public List<float> Sizefactor = new List<float> { 0.3f, 0.3f, 1.0f };

     // optional: define a scale between GAMA and Unity for the location given
     public float GamaCRSCoefX = 1.0f;
     public float GamaCRSCoefY = 1.0f;
     public float GamaCRSOffsetX = 0f;
     public float GamaCRSOffsetY = 0f;


     //Y scale for the ground
     public float groundY = 1.0f;

     //Y-offset to apply to the background geometries
     public float offsetYBackgroundGeom = 0.1f;

     private List<Dictionary<int, GameObject>> agentMapList ;

     private TcpClient socketConnection;
     private Thread clientReceiveThread;

     private bool initialized = false;
     private bool playerPositionUpdate = false; //Uppdate position of player in Unity from GAMA

     private string message ="";

     private bool defineGroundSize = false;

     private static bool receiveInformation = true;
     private static bool readySendPlayerPosition = false;

     private static bool timerFinish = false;

     private WorldJSONInfo infoWorld = null;

     private ConnectionParameter parameters = null;

     private ConnectionClass classIndicators = null;

     private List<GAMAGeometry> geoms;

     private static System.Timers.Timer aTimer;

     private CoordinateConverter converter;

     private PolygonGenerator polyGen;

     private int village_id;

     private static DisplayManagement dm;

     private WasteCollectionInfo wci;
     private ParamPNJ pPNJ1;

     private ModeConfig mc;

     private HelpManagement hm;

     private bool restart;

     private bool help = false;


     // Start is called before the first frame update
     void Start()
     {
         DisplayMessage("Start");
         agentMapList = new List<Dictionary<int, GameObject>>();
         foreach (GameObject i in Agents) 
         {
             agentMapList.Add(
                 new Dictionary<int, GameObject>());
         }
         Debug.Log("START WORLD");
         DisplayMessage("IP: " + ip + " port: " + port);
         ConnectToTcpServer();

         dm = WasteDisplayM.GetComponent<DisplayManagement>();
         wci = WasteCollectionI.GetComponent<WasteCollectionInfo>();
         mc = ModeConfigM.GetComponent<ModeConfig>();
         pPNJ1 = PNJ1.GetComponent<ParamPNJ>();
         hm = HelpM.GetComponent<HelpManagement>();

         restart = false;
     }

     private void Update()
     {

         if (classIndicators != null)
         {
             classIndicators.displaySolidClass(classIndicators.solidwasteSoilClass[village_id], classIndicators.solidwasteCanalClass[village_id]);
             classIndicators.displayWaterClass(classIndicators.waterwasteClass[village_id]);
             classIndicators.displayProductionClass(classIndicators.productionClass[village_id]);
             classIndicators.displayWaterColor(classIndicators.waterwasteClass[village_id]);
         }
         // if (restart)
         // {
         //     Debug.Log("enter");
         //     SceneManager.LoadScene("Assets/Scenes/RAC_MainScene.unity");
         //     Debug.Log("done");
         // }

         if (help) {
             hm.PanelTuto.SetActive(true);
             hm.Complete_XR.transform.position = hm.start_pos;
             hm.Complete_XR.transform.rotation = hm.start_rot;
             //hm.XR.transform.position = hm.start_pos_xr;
             //hm.XR.transform.rotation = hm.start_rot_xr;
             help = false;
         }
        //DisplayMessage("Update");
         if (text != null && message != null)
         {
             if (message.Contains("agents"))
             {
                 DisplayMessage("Received agents: " + message.Length);
             }
         }
     }
     public int cpt = 0;
     // Specify what you want to happen when the Elapsed event is raised.
     private static void OnTimedEvent(object source, ElapsedEventArgs e)
     {
         receiveInformation = true;
         timerFinish = true;
         aTimer.Stop();
         aTimer = null;

     }

     void FixedUpdate()
     {
         cpt++;
        // DisplayMessage("parameters: " + parameters + " cpt:"  + cpt);

         if (parameters != null && geoms != null)
         {
             foreach (GAMAGeometry g in geoms)
             {
                 // if (polyGen == null) polyGen = new PolygonGenerator(converter, offsetYBackgroundGeom);
                 polyGen.GeneratePolygons(g);
             }
             geoms = null;
         } 
         if (parameters != null && Ground != null && !defineGroundSize)
         {
             Vector3 ls = converter.fromGAMACRS(parameters.world[0], parameters.world[1]);
             if (ls.z < 0)
                 ls.z = -ls.z;
             if (ls.x < 0)
                 ls.x = -ls.x;
             ls.y = groundY;
             Ground.transform.localScale = ls;

             Vector3 ps = converter.fromGAMACRS(parameters.world[0]/2, parameters.world[1]/2);
             ps.y = -groundY;

             Ground.transform.position = ps;
             defineGroundSize = true;
             if (Player != null && playerPositionUpdate)
             {
                 Vector3 pos = converter.fromGAMACRS(parameters.position[0], parameters.position[1]);
                 Player.transform.position = pos;
             }
             if (parameters.physics && Player != null)
             {
                 if (Player.GetComponent<Rigidbody>() == null)
                 {
                     Player.AddComponent<Rigidbody>();
                 }
             } 
             else
             {
                 if (Player.GetComponent<Rigidbody>() != null)
                 {
                     Destroy(Player.GetComponent<Rigidbody>());
                 }
             }
         }
         if (timerFinish)
         {
             timerFinish = false;

             SendMessageToServer("ready");

             return;
         }
       
         if (Player != null && playerPositionUpdate && parameters != null)
         {
             Player.transform.position = converter.fromGAMACRS(parameters.position[0], parameters.position[1]);
             playerPositionUpdate = false;
             receiveInformation = false;
             if (parameters.delay > 0)
             {
                 aTimer = new System.Timers.Timer(parameters.delay);
                 aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

                 aTimer.AutoReset = false;
                 aTimer.Enabled = true;
             } else
             {
                 receiveInformation = true;
             }
           

         }
         if (initialized && Player != null && receiveInformation && readySendPlayerPosition)
         {
             SendPlayerPosition();
         }
         if (initialized && wci.sendInfoWasteCollection && receiveInformation)
         {
             SendChoiceAndNbWaste();
             wci.sendInfoWasteCollection = false;
         }
         if (initialized && wci.pointInterstVisited && receiveInformation)
         {
             Debug.Log("enter");
             SendEndDialogue();
             wci.pointInterstVisited = false;
         }
         if (infoWorld != null && receiveInformation)
         {
             UpdateAgentList();

             infoWorld = null;

         }
         if (PNJ1 != null && pPNJ1.readySendPosition){
             SendInitPNJPos(PNJ1, pPNJ1);
             pPNJ1.readySendPosition = false;
         }
     }

   
     private void SendPlayerPosition()
     {
         Vector2 vF = new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z);
         Vector2 vR = new Vector2(transform.forward.x, transform.forward.z);
         vF.Normalize();
         vR.Normalize();
         float c = vF.x * vR.x + vF.y * vR.y;
         float s = vF.x * vR.y - vF.y * vR.x;

         double angle = ((s >= 0) ? 1.0 : -1.0) * (180 / Math.PI) * Math.Acos(c) * parameters.precision;

         List<int> p = converter.toGAMACRS(Player.transform.position); 
         SendMessageToServer("{\"position\":[" + p[0] + "," + p[1] + "],\"rotation\": " + (int)angle + "}");
         // SendMessageToServer("{\"position\":[" + p[0] + "," + p[1] + "]" + "}");
     }

     private void SendChoiceAndNbWaste()
     {   
         int choice_int = -1;  0 : ground, 1 : river, -1 : default
         if (wci.ground_choice) 
         {
             choice_int = 0;
         } 
         else if (wci.river_choice)
         {
             choice_int = 1;
         }
         SendMessageToServer("{\"choice\": " + choice_int + ",\"nb_waste\": " + wci.nb_waste + "}");
     }

     private void SendInitPNJPos(GameObject Object, ParamPNJ pPNJ){
         List<int> p = converter.toGAMACRS(Object.transform.position);
         SendMessageToServer("{\"pnj_pos\":[" + p[0] + "," + p[1] + "], \"pnj_id\":"+ pPNJ.id +"}");
     }

     private void SendEndDialogue(){
         SendMessageToServer("{\"point_of_interest\": " + wci.pointInterestIndex + "}");
     }

     private void UpdateAgentList()
     {
         if (infoWorld.position.Count == 2)
         {
             parameters.position = infoWorld.position;
             playerPositionUpdate = true;
         }
         foreach (Dictionary<int, GameObject> agentMap in agentMapList) { 
             foreach (GameObject obj in agentMap.Values)
             {
                 obj.SetActive(false);
             }
         }

        foreach (AgentInfo pi in infoWorld.agents)
         {
             int speciesIndex = pi.v[0];
             GameObject Agent = Agents[speciesIndex];
             int id = pi.v[1];
             GameObject obj = null;
             Dictionary<int, GameObject> agentMap = agentMapList[speciesIndex];
             if (!agentMap.ContainsKey(id))
             {
                 obj = Instantiate(Agent);
                 float scale = Sizefactor[speciesIndex];
                 obj.transform.localScale = new Vector3(scale, scale, scale);
                 obj.SetActive(true);

                 agentMap.Add(id, obj);
             }
             else
             {
                 obj = agentMap[id];
             }

            
              Vector3 pos = converter.fromGAMACRS(pi.v[2], pi.v[3]);
              pos.y = YValues[speciesIndex];
             float rot = rotationsCoeff[speciesIndex] * (pi.v[4] / parameters.precision) + rotations[speciesIndex];
             obj.transform.SetPositionAndRotation(pos,Quaternion.AngleAxis(rot, Vector3.up));

             obj.SetActive(true);


         }
         foreach (Dictionary<int, GameObject> agentMap in agentMapList)
         {
             List<int> ids = new List<int>(agentMap.Keys);
             foreach (int id in ids)
             {
                 GameObject obj = agentMap[id];
                 if (!obj.activeSelf)
                 {
                     obj.transform.position = new Vector3(0, -100, 0);
                     agentMap.Remove(id);
                     GameObject.Destroy(obj);
                 }
             }
         }
     }


   

     // protected override void ManageMessage(string mes)
     protected void ManageMessage(string mes)
     {
         Debug.Log(mes);
         if (mes.Contains("precision"))
         {
             parameters = ConnectionParameter.CreateFromJSON(mes);
             converter = new CoordinateConverter(parameters.precision, GamaCRSCoefX, GamaCRSCoefY, GamaCRSOffsetX, GamaCRSOffsetY);
             SendMessageToServer("ok");
             initialized = true;
             //playerPositionUpdate = true;
             mc.language = parameters.language;
             mc.mode = parameters.mode;
             if (mc.mode == "Demo_01")
             {
                 village_id = 2;
             }
             else if (mc.mode == "Demo_2")
             {
                 village_id = 1;
             }
         }
         else if (mes.Contains("points"))
         {
             GAMAGeometry g = GAMAGeometry.CreateFromJSON(mes);
             if (geoms == null)
             {
                 geoms = new List<GAMAGeometry>();
             }
             geoms.Add(g);
         }
         else if (mes.Contains("agents") && parameters != null)
         {
             infoWorld = WorldJSONInfo.CreateFromJSON(mes);

         }
         else if (mes.Contains("solidwasteSoilClass")){

             classIndicators = ConnectionClass.CreateFromJSON(mes, dm);
         }
         else if (mes.Contains("Enter_or_exit_VR")){
             readySendPlayerPosition = !readySendPlayerPosition;
             pPNJ1.readySendPosition = true;
         }
         // else if (mes.Contains("Restart")){
         //     restart = true;
         // }
         else if (mes.Contains("Help")){
             help = true;
         }

         if (text != null)
             message = mes;

     }

     private void SendMessageToServer(string m) {

     }

    */
}
