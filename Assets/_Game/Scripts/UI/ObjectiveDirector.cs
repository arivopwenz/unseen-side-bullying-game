using UnityEngine;
using BullyingGame.NPC;
using BullyingGame.Events;

namespace BullyingGame.UI
{
    /// <summary>Mengatur HUD Bully (banner, marker tanah, minimap, kartu judul, jam,
    /// bar Bima) untuk fase di luar sekuens kantin (tur, Misi 2/3, ajakan ke kantin).
    /// Selama sekuens kantin berjalan, CanteenSequenceDirector yang memegang HUD.</summary>
    public class ObjectiveDirector : MonoBehaviour
    {
        private GuideController guide;
        private ClassroomCutsceneDriver classroom;
        private Mission2Driver m2;
        private CanteenSequenceDirector canteen;
        private Transform canteenMark;
        private Transform[] supportZones = new Transform[3];

        private float timer;
        private string lastPhase = "";

        private void Start()
        {
            var guides = FindObjectsByType<GuideController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (guides.Length > 0) guide = guides[0];
            var rooms = FindObjectsByType<ClassroomCutsceneDriver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (rooms.Length > 0) classroom = rooms[0];
            var m2s = FindObjectsByType<Mission2Driver>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (m2s.Length > 0) m2 = m2s[0];
            var cans = FindObjectsByType<CanteenSequenceDirector>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (cans.Length > 0) canteen = cans[0];
            var cm = GameObject.Find("BimaMark_Kantin");
            if (cm != null) canteenMark = cm.transform;
            string[] names = new string[] { "SupportBima", "SupportNerdSosial", "SupportNerdMateriil" };
            for (int i = 0; i < names.Length; i++)
            {
                var z = GameObject.Find(names[i]);
                if (z != null) supportZones[i] = z.transform;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < 0.25f) return;
            timer = 0f;
            Refresh();
        }

        private bool AnyTrue(bool[] arr)
        {
            if (arr == null) return false;
            foreach (var b in arr) if (b) return true;
            return false;
        }

        private int FirstFalse(bool[] arr)
        {
            if (arr == null) return 0;
            for (int i = 0; i < arr.Length; i++) if (!arr[i]) return i;
            return arr.Length - 1;
        }

        private void Refresh()
        {
            if (canteen != null && canteen.HasStarted && !canteen.IsComplete) return;

            bool m2active = m2 != null && (AnyTrue(m2.Observed) || m2.Mission2Complete);
            bool m3active = m2 != null && (AnyTrue(m2.Supported) || m2.Mission3Complete);
            bool canteenDone = canteen != null && canteen.IsComplete;

            if (m2active || m3active || canteenDone)
            {
                if (m2 != null && !m2.Mission2Complete)
                {
                    bool[] obs = m2.Observed != null && m2.Observed.Length >= 3 ? m2.Observed : new bool[] { false, false, false };
                    if (lastPhase != "MISI 2") { MissionTitleCard.ShowTitle("MISI 2", "Observasi"); lastPhase = "MISI 2"; }
                    string[] m2items = new string[] { "Amati kejadian pertama", "Amati kejadian kedua", "Amati kejadian ketiga" };
                    MissionBanner.SetObjective(m2items[FirstFalse(obs)]);
                    Vector3[] m2places = new Vector3[] {
                        new Vector3(-16.4f, 0f, -5.1f),
                        new Vector3(-3.5f, 0f, 6.5f),
                        new Vector3(-0.5f, 0f, 6.5f) };
                    Vector3 m2place = m2places[Mathf.Clamp(FirstFalse(obs), 0, 2)];
                    GroundMarker.SetTarget(m2place);
                    Minimap.SetMissionTarget(m2place);
                    ClockHUD.SetSchedule("Istirahat");
                    CompanionBar.SetCompanion("", 0f, false);
                }
                else if (m2 != null && !m2.Mission3Complete)
                {
                    bool[] sup = m2.Supported != null && m2.Supported.Length >= 3 ? m2.Supported : new bool[] { false, false, false };
                    if (lastPhase != "MISI 3") { MissionTitleCard.ShowTitle("MISI 3", "Uluran Tangan"); lastPhase = "MISI 3"; }
                    string[] m3items = new string[] { "Dukung Bima", "Dukung Nerd (sosial)", "Dukung Nerd (materiil)" };
                    MissionBanner.SetObjective(m3items[FirstFalse(sup)]);
                    Transform best = null;
                    float bestD = float.MaxValue;
                    var pp = GameObject.FindGameObjectWithTag("Player");
                    for (int i = 0; i < supportZones.Length; i++)
                    {
                        if (supportZones[i] == null || sup[i]) continue;
                        float d = pp != null ? Vector3.Distance(pp.transform.position, supportZones[i].position) : 0f;
                        if (d < bestD) { bestD = d; best = supportZones[i]; }
                    }
                    if (best != null) { GroundMarker.SetTarget(best.position); Minimap.SetMissionTarget(best.position); }
                    else { GroundMarker.ClearTarget(); Minimap.ClearMissionTarget(); }
                    ClockHUD.SetSchedule("Istirahat");
                    CompanionBar.SetCompanion("", 0f, false);
                }
                else
                {
                    if (lastPhase != "MISI 3 DONE") { MissionTitleCard.ShowTitle("MISI 3", "Selesai"); lastPhase = "MISI 3 DONE"; }
                    MissionBanner.SetObjective("Misi 3 selesai");
                    GroundMarker.ClearTarget();
                    Minimap.ClearMissionTarget();
                    ClockHUD.SetSchedule("Istirahat");
                    CompanionBar.SetCompanion("", 0f, false);
                }
                return;
            }

            if (classroom != null && classroom.Finished)
            {
                if (lastPhase != "KANTIN INVITE") { MissionTitleCard.ShowTitle("OBJECTIVE 2", "Kantin"); lastPhase = "KANTIN INVITE"; }
                MissionBanner.SetObjective("Ikuti Bima ke kantin");
                Vector3 canteenPlace = canteenMark != null ? canteenMark.position : new Vector3(-39.5f, 0f, -1f);
                GroundMarker.SetTarget(canteenPlace);
                Minimap.SetMissionTarget(canteenPlace);
                ClockHUD.SetSchedule("Istirahat");
                CompanionBar.SetCompanion("Bima", 1f, true);
                return;
            }

            if (lastPhase != "MISI 1") { MissionTitleCard.ShowTitle("MISI 1", "Perjalanan"); lastPhase = "MISI 1"; }
            MissionBanner.SetObjective("Ikuti Bima ke kelas");
            Vector3 classPlace = classroom != null ? classroom.transform.position : new Vector3(-20f, 0f, 12.5f);
            GroundMarker.SetTarget(classPlace);
            Minimap.SetMissionTarget(classPlace);
            ClockHUD.SetSchedule("Pagi");
            CompanionBar.SetCompanion("Bima", 1f, true);
        }
    }
}
