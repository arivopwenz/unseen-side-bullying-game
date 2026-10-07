using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BullyingGame.Core;
using BullyingGame.Quest;
using BullyingGame.Dialogue;

public static class FiveChapterAssets
{
    const string Folder="Assets/_Game/Data/StoryPrototype/";
    static T Asset<T>(string name) where T:ScriptableObject
    {
        string path=Folder+name+".asset";
        var value=AssetDatabase.LoadAssetAtPath<T>(path);
        if(value==null) { value=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(value,path); }
        return value;
    }
    static void Text(SerializedObject so,string field,string text) => so.FindProperty(field).stringValue=text;
    static DialogueData Dialogue(string name,params string[] lines)
    {
        var data=Asset<DialogueData>(name);data.lines=new DialogueLine[lines.Length];
        for(int i=0;i<lines.Length;i++) { int split=lines[i].IndexOf('|');data.lines[i]=new DialogueLine{speakerName=lines[i].Substring(0,split),text=lines[i].Substring(split+1)}; }
        EditorUtility.SetDirty(data);return data;
    }
    static QuestData Quest(string id,string title,QuestData previous,params string[] objectives)
    {
        var data=id=="ch01_book" ? AssetDatabase.LoadAssetAtPath<QuestData>("Assets/_Game/Data/Quests/Quest_Level01_LostNotebook.asset") : Asset<QuestData>("Quest_"+id);
        if(id!="ch01_book") data.questId=id;
        data.questTitle=title;data.questDescription=title;
        data.prerequisites=previous==null ? new QuestData[0] : new[]{previous};
        data.objectives=new QuestObjective[objectives.Length];
        for(int i=0;i<objectives.Length;i++) { var parts=objectives[i].Split('|');data.objectives[i]=new QuestObjective{objectiveId=parts[0],description=parts[1],requiredAmount=1}; }
        var so=new SerializedObject(data);Text(so,"completionMessage","Tujuan selesai. Langkah berikutnya akan muncul.");so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(data);return data;
    }
    static void Chapter(int number,string title,string player,POVType pov,string opening,string closing,QuestData[] missions,params string[] questions)
    {
        var data=Asset<NarrativeChapterData>("Chapter0"+number);var so=new SerializedObject(data);
        Text(so,"chapterTitle",title);Text(so,"playerName",player);so.FindProperty("perspective").enumValueIndex=(int)pov;
        Text(so,"closingMessage",closing);so.FindProperty("openingDialogue").objectReferenceValue=AssetDatabase.LoadAssetAtPath<DialogueData>(Folder+opening+".asset");
        var quests=so.FindProperty("missions");quests.arraySize=missions.Length;
        for(int i=0;i<missions.Length;i++) quests.GetArrayElementAtIndex(i).objectReferenceValue=missions[i];
        var quiz=so.FindProperty("reflections");quiz.arraySize=questions.Length;
        for(int i=0;i<questions.Length;i++)
        {
            var fields=questions[i].Split('|');var q=quiz.GetArrayElementAtIndex(i);
            q.FindPropertyRelative("questionText").stringValue=fields[0];var options=q.FindPropertyRelative("options");options.arraySize=3;
            for(int j=0;j<3;j++) options.GetArrayElementAtIndex(j).stringValue=fields[j+1];
            q.FindPropertyRelative("correctOptionIndex").intValue=int.Parse(fields[4]);q.FindPropertyRelative("educationalExplanation").stringValue=fields[5];
        }
        so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(data);
    }
    public static string Run()
    {
        if(Application.isPlaying) throw new Exception("Edit Mode required");
        Dialogue("Opening_Victim",
            "Narasi|Halaman sekolah, sebelum kegiatan dimulai. Arga masih mencari tempat di antara teman-teman barunya.",
            "Arga|Kalau aku membantu mereka, mungkin besok aku tidak duduk sendirian lagi.",
            "Raka|Dimas, uang jajanmu sini. Anggap saja biaya supaya kamu tidak diganggu.",
            "Dimas|Aku sudah bilang tidak. Itu uang untuk pulang. Tolong jangan ambil.",
            "Bima|Sudah, Rak. Dia menangis. Aku mulai merasa ini kelewatan.",
            "Raka|Kalian terlalu serius. Aku cuma bercanda.",
            "Nara|Aku melihat semuanya. Aku ingin maju, tetapi takut mereka berbalik menyerangku.",
            "Narasi|Saat satu kelompok memegang kuasa, menolak bisa terasa berbahaya. Diam atau menangis bukan persetujuan.",
            "Arga|Aku akan mendekati Dimas dulu. Dia tidak harus sendirian.");
        Dialogue("Opening_Witness",
            "Narasi|Taman sekolah, saat istirahat. Nara mengingat kejadian di halaman tadi pagi.",
            "Nara|Minggu lalu, teman yang membela Dimas juga diejek. Sejak itu aku memilih pura-pura tidak melihat.",
            "Dimas|Waktu kalian tertawa atau diam, aku merasa tidak ada yang akan percaya padaku.",
            "Nara|Aku tidak tertawa, tetapi aku juga tidak mendekat. Aku takut kehilangan teman-temanku.",
            "Arga|Kamu tidak perlu melawan kelompok itu sendirian. Menemani Dimas dan mencari bantuan juga berarti bertindak.",
            "Nara|Aku akan mendengarkan dulu. Lalu aku bisa meminta bantuan tanpa menyebarkan ceritanya ke semua orang.",
            "Narasi|Ketakutan saksi dapat dipahami. Pilihan berikutnya tetap dapat mengurangi atau memperpanjang dampak perundungan.");
        Dialogue("Opening_Bully",
            "Narasi|Lapangan sekolah. Raka mendengar kembali kalimat yang sering ia dengar: 'Jangan kalah. Jangan terlihat lemah.'",
            "Raka|Di rumah aku sering dibandingkan. Di kelompok ini, aku ingin jadi orang yang tidak bisa diremehkan.",
            "Bima|Jadi kamu membuat Dimas takut supaya kami menganggapmu kuat?",
            "Raka|Awalnya aku mengira tawa kalian berarti kalian setuju. Aku terus mengulanginya agar tetap dianggap pemimpin.",
            "Dimas|Aku tidak merasa itu lelucon. Aku takut bertemu kalian setiap pagi.",
            "Raka|Aku menyebutnya bercanda karena lebih mudah daripada mengakui aku menyakitimu.",
            "Narasi|Tekanan di rumah dan kebutuhan diterima menjelaskan sebagian latar Raka. Keduanya tidak membenarkan pemalakan atau intimidasi.",
            "Raka|Aku harus berhenti, mendengar dampaknya, dan bertanggung jawab. Dimas tidak wajib memaafkanku.");
        Dialogue("Opening_Boundaries",
            "Narasi|Taman lingkungan, sepulang sekolah. Arga duduk dengan Dimas, jauh dari keramaian kelompok Raka.",
            "Arga|Aku murid baru. Aku takut ditinggalkan, jadi aku selalu mengiyakan permintaan Raka. Setelah kejadian buku, aku merasa ikut terjebak.",
            "Dimas|Mereka memilih orang yang belum punya banyak dukungan. Bukan karena kita layak diperlakukan begitu.",
            "Arga|Sekarang ada pesan di grup yang mengejekku. Aku ingin menghapus semuanya dan tidak pergi sekolah lagi.",
            "Nara|Kita bisa menyimpan bukti tanpa menyebarkannya. Kamu yang menentukan siapa boleh melihat.",
            "Arga|Aku ingin Bu Nadia tahu. Aku juga ingin punya batas: bantuan bukan kewajiban untuk menuruti ancaman.",
            "Narasi|Rasa takut dan kesulitan menolak tumbuh di bawah tekanan. Tanggung jawab perundungan tetap ada pada pelaku.");
        Dialogue("Opening_Together",
            "Narasi|Halaman sekolah setelah kegiatan. Raka, Dimas, Arga, dan Nara bertemu dengan pendampingan Bu Nadia.",
            "Nara|Aku pernah memilih diam. Hari ini aku ingin menjadi saksi yang bisa dipercaya, bukan orang yang menjadikan kejadian ini tontonan.",
            "Raka|Aku sudah menghentikan pemalakan. Aku perlu mengembalikan uang dan memperbaiki tindakanku, bukan meminta kalian melupakan.",
            "Dimas|Aku belum siap dekat denganmu. Aku ingin ada orang dewasa saat kita membahasnya.",
            "Raka|Aku akan menghormati itu. Aku tidak akan memaksamu menerima maafku.",
            "Arga|Kami juga butuh rencana kalau kejadian ini berulang, bukan hanya janji hari ini.",
            "Bu Nadia|Kita catat langkahnya, siapa yang mendampingi, dan bagaimana melaporkan tanpa membuat korban makin terpapar.",
            "Narasi|Perubahan tidak selesai dalam satu percakapan. Komitmen perlu tindakan, batas yang dihormati, dan tindak lanjut.");
        Dialogue("Dimas_Listen","Dimas|Tadi mereka mengambil uangku. Kalau aku menangis, mereka malah menyebutku lemah.","Arga|Aku percaya kamu. Mau aku duduk di sini dulu?","Dimas|Iya. Jangan langsung panggil mereka. Aku masih takut.","Arga|Kita mulai dengan yang membuatmu merasa aman. Aku juga akan mencari bantuan.");
        Dialogue("Raka_Request","Raka|Arga, buku catatanku hilang di halaman. Carikan. Anak baru harus bisa diandalkan.","Arga|Aku bisa membantu mencari. Tetapi aku bukan pesuruhmu.","Raka|Cepat saja. Buku itu penting.","Narasi|Arga menerima tugas mencari buku. Membantu tidak berarti menyetujui intimidasi.");
        Dialogue("Raka_Return","Arga|Ini bukumu. Aku mengembalikannya karena itu barangmu, bukan karena ancamanmu benar.","Raka|Kamu sekarang berani bicara begitu?","Arga|Aku ingin kita saling menghormati. Aku selesai membantu mencari buku.","Narasi|Buku kembali kepada pemiliknya. Arga mulai membedakan bantuan dengan tuntutan di bawah tekanan.");
        Dialogue("Dimas_Extortion","Dimas|Mereka meminta uang lagi besok. Katanya kalau lapor, aku akan dikucilkan.","Arga|Ancaman itu membuatmu sulit menolak. Ini bukan utang yang harus kamu bayar.","Dimas|Aku ingin dibantu, tetapi takut ceritaku jadi bahan tertawaan.","Arga|Kita bicara dengan Bu Nadia secara terbatas. Aku bisa menemanimu, dan kamu boleh menentukan yang ingin disampaikan.");
        Dialogue("Teacher_Extortion","Bu Nadia|Terima kasih sudah mencari bantuan. Apa yang paling mendesak sekarang?","Arga|Dimas dipalak dan diancam. Ia ingin ceritanya hanya didengar orang yang membantu.","Bu Nadia|Kita lindungi dulu keselamatan dan kebutuhannya. Saya akan mendampingi penanganan tanpa mempertemukannya sendirian dengan kelompok itu.","Arga|Saya akan menemaninya ke area aman di dekat taman.");
        Dialogue("Dimas_Escort","Arga|Bu Nadia sudah tahu. Mau berjalan ke area aman bersamaku?","Dimas|Mau. Tolong jangan berjalan terlalu jauh di depan.","Arga|Aku tetap dekat. Kalau kamu ingin berhenti, bilang saja.");
        Dialogue("Dimas_Safe","Dimas|Di sini aku bisa bernapas sedikit lebih lega. Terima kasih tidak memaksaku langsung melawan.","Arga|Kita belum menyelesaikan semuanya. Tapi kamu punya pendamping sekarang.","Dimas|Aku ingin Nara tahu apa yang kulihat dan kurasakan, supaya dia tidak menganggapnya bercanda.");
        Dialogue("Nara_Witness","Nara|Aku melihat pemalakan itu. Aku diam karena takut menjadi sasaran berikutnya.","Arga|Kamu bisa membantu tanpa menghadapi mereka sendirian. Mau ikut menyampaikan yang kamu lihat kepada Bu Nadia?","Nara|Mau. Aku akan mengatakan yang benar-benar kulihat, bukan menambahkan dugaan.");
        Dialogue("Teacher_Plan","Arga|Nara bersedia menjadi saksi. Dimas meminta didampingi dan menjaga ceritanya tetap terbatas.","Bu Nadia|Baik. Saya akan menindaklanjuti dan memeriksa keadaan mereka. Kalian tidak perlu menyelesaikan ini sendiri.","Arga|Kalau mereka mengancam lagi, kami akan menghubungi Ibu dan mencari tempat aman.");
        Dialogue("Witness_Check","Nara|Dimas, aku melihat kejadian tadi. Aku menyesal pura-pura tidak melihat.","Dimas|Aku tidak butuh kamu mempermalukan mereka di depan semua orang. Aku ingin didengar dan merasa aman.","Nara|Apa aku boleh menemanimu sekarang?","Dimas|Boleh. Terima kasih bertanya dulu.");
        Dialogue("Witness_Arga","Nara|Arga, aku takut kalau membela Dimas aku juga akan diejek.","Arga|Aku juga takut. Kita bisa mencari dukungan bersama, tanpa membuat Dimas jadi tontonan.","Nara|Aku akan bicara pada Bu Nadia tentang kejadian yang kulihat dan kebutuhan Dimas.");
        Dialogue("Witness_Teacher","Nara|Saya melihat Raka meminta uang dan mengancam Dimas. Saya sempat diam karena takut.","Bu Nadia|Terima kasih menyampaikan fakta. Kamu boleh datang bersama pendamping; kamu tidak harus menghadapi Raka sendiri.","Nara|Saya ingin membantu tanpa menyebarkan nama dan cerita Dimas ke grup kelas.","Bu Nadia|Itu penting. Tanyakan persetujuannya sebelum membagikan informasi di luar penanganan ini.");
        Dialogue("Witness_Consent","Nara|Bu Nadia bersedia mendampingi. Apa yang boleh aku sampaikan sebagai saksi?","Dimas|Ceritakan pemalakan yang kamu lihat. Jangan kirim video tangisku atau membahasnya di grup.","Nara|Aku akan menghormati batas itu. Kalau ada yang bertanya, aku tidak akan menyebarkan ceritamu.");
        Dialogue("Witness_Boundaries","Arga|Membantu bukan berarti kita bebas mengambil keputusan untuk Dimas.","Nara|Aku sudah meminta persetujuannya. Aku hanya akan menyampaikan fakta kepada pendamping.","Arga|Kalau situasi memanas, kita pergi ke tempat aman dan panggil bantuan. Itu tetap tindakan berani.");
        Dialogue("Bima_Listen","Bima|Aku tertawa supaya tidak dibilang merusak suasana. Tapi Dimas sudah berkali-kali meminta berhenti.","Raka|Jadi tawa kalian tidak berarti semua orang setuju?","Bima|Tidak. Aku ikut membuatnya terasa diterima. Aku juga harus berhenti mendukungnya.","Raka|Aku harus mendengar dampaknya, bukan mencari alasan dari reaksi kalian.");
        Dialogue("Bully_Accountability","Raka|Saya sering merasa diremehkan di rumah. Di kelompok ini saya ingin mengendalikan orang lain.","Bu Nadia|Perasaan itu perlu dibantu. Tetapi kamu bertanggung jawab atas pemalakan dan ancaman yang kamu lakukan.","Raka|Saya akan berhenti meminta uang dan mengembalikan yang sudah saya ambil.","Bu Nadia|Kita buat langkah yang dapat diperiksa. Dimas tidak wajib memaafkan agar kamu mulai berubah.");
        Dialogue("Bully_Consent","Raka|Dimas, aku ingin mengembalikan uang dan memperbaiki tindakanku. Apa kamu bersedia membahasnya dengan pendamping?","Dimas|Dengan Bu Nadia, iya. Jangan mendatangiku sendirian dan jangan menuntut aku memaafkanmu.","Raka|Aku akan menghormatinya. Aku tidak akan menghubungimu di luar cara yang kamu setujui.");
        Dialogue("Bully_Plan","Raka|Dimas hanya bersedia bicara dengan pendamping. Saya menyetujui batas itu.","Bu Nadia|Langkah pertama: hentikan ancaman, kembalikan uang melalui pendamping, dan hentikan ejekan di kelompok.","Raka|Saya akan menyampaikan batas itu kepada teman-teman dan datang untuk tindak lanjut.","Bu Nadia|Saya akan memeriksa tindakanmu. Perubahan perlu bukti, bukan sekadar janji.");
        Dialogue("Park_Check","Arga|Aku merasa bersalah karena dulu mengikuti permintaan Raka. Aku juga mulai takut membaca grup kelas.","Dimas|Kamu boleh meninjau pilihanmu tanpa menyalahkan diri atas tindakan mereka. Aku bisa mendengarkan.","Arga|Aku ingin ditemani saat mencari bantuan. Aku belum siap bicara dengan Raka sendirian.");
        Dialogue("Park_Evidence","Nara|Pesan yang mengejekmu bisa disimpan sebagai bukti. Kita tidak perlu mengirim ulang ke grup.","Arga|Simpan untuk Bu Nadia saja. Aku tidak ingin orang lain melihat nama dan pesanku.","Nara|Baik. Kita catat waktu dan akun pengirim, lalu serahkan secara terbatas dengan persetujuanmu.");
        Dialogue("Park_Support","Arga|Ini pesan yang berulang dan membuat saya takut. Saya ingin bantuan tanpa mempublikasikannya.","Bu Nadia|Saya akan mendampingi penanganan. Simpan bukti, gunakan fitur lapor atau blokir bila aman, dan hubungi pendamping saat ancaman meningkat.","Arga|Saya ingin punya jalur laporan yang jelas kalau hal ini berulang.");
        Dialogue("Park_Boundary","Arga|Aku mau membantu teman. Aku tidak mau membantu karena diancam atau dipaksa menyerahkan uang.","Dimas|Kita boleh berkata tidak dan pergi ke tempat aman. Kita juga boleh meminta pendamping.","Arga|Besok aku akan menggunakan batas itu. Kalau aku takut, aku akan mencari bantuan, bukan menanggungnya sendiri.");
        Dialogue("Together_Facts","Nara|Aku ingin memeriksa catatanku sebelum memberi keterangan. Aku melihat pemalakan, tetapi tidak tahu semua percakapan sebelumnya.","Arga|Katakan bagian yang benar-benar kamu lihat. Jangan menambahkan rumor atau menyebarkan bukti pribadi.","Nara|Aku akan memisahkan fakta, hal yang belum kuketahui, dan kebutuhan pendampingan.");
        Dialogue("Together_Boundaries","Nara|Dimas, pertemuan ini didampingi Bu Nadia. Apakah batas yang kamu minta masih sama?","Dimas|Iya. Jangan paksa aku akrab dengan Raka. Aku ingin tahu siapa yang dapat kuhubungi kalau ia mengulanginya.","Nara|Aku akan menyampaikan batas itu dan tetap menanyakan keadaanmu setelah pertemuan.");
        Dialogue("Together_Commitment","Nara|Raka, apa tindakan yang sudah kamu lakukan? Dimas tidak meminta kedekatan atau maaf sebagai syarat.","Raka|Aku berhenti meminta uang, menyerahkan pengembaliannya kepada Bu Nadia, dan meminta Bima menghentikan ejekan di grup.","Nara|Kami akan melihat tindak lanjutnya. Jangan hubungi Dimas di luar batas yang ia setujui.","Raka|Aku mengerti. Aku akan datang untuk pemeriksaan berikutnya bersama pendamping.");
        Dialogue("Together_FollowUp","Nara|Fakta sudah dicatat, batas Dimas sudah disampaikan, dan komitmen Raka perlu diperiksa.","Bu Nadia|Saya akan menjadwalkan tindak lanjut. Korban boleh melapor lagi dan perubahan pelaku tidak menghapus dampak yang sudah terjadi.","Nara|Saya akan mendampingi dengan persetujuan, tidak menyebarkan cerita, dan meminta bantuan saat situasi tidak aman.","Bu Nadia|Itulah rencana bersama kita. Tidak seorang pun harus menangani perundungan sendirian.");
        var chapters=new List<QuestData[]>();QuestData previous=null;
        Func<string,string,string[],QuestData> add=(id,title,objectives)=> { var q=Quest(id,title,previous,objectives);previous=q;return q; };
        chapters.Add(new[]{
            add("ch01_friend","Dengarkan Dimas",new[]{"listen_friend|Bicara dengan Dimas di halaman"}),
            add("ch01_book","Buku catatan dan batas",new[]{"accept_request|Dengarkan permintaan Raka","find_notebook|Temukan buku di halaman","return_notebook|Kembalikan buku kepada Raka"}),
            add("ch01_extortion","Jangan hadapi pungli sendirian",new[]{"hear_extortion|Dengarkan ancaman yang Dimas alami","seek_support|Minta bantuan Bu Nadia di luar kelas"}),
            add("ch01_escort","Temani ke area aman",new[]{"agree_escort|Ajak Dimas berjalan bersama","reach_safe_zone|Temani Dimas menuju lingkaran hijau","check_in|Tanyakan keadaan Dimas setelah tiba"}),
            add("ch01_witness","Dukungan dan saksi",new[]{"speak_witness|Dengarkan Nara sebagai saksi","plan_support|Susun tindak lanjut bersama Bu Nadia"}),
            add("ch01_reflection","Refleksi: dukungan yang aman",new[]{"reflect|Selesaikan dua pertanyaan refleksi"})});
        chapters.Add(new[]{
            add("ch02_check","Dari diam menjadi hadir",new[]{"check_friend|Minta izin untuk menemani Dimas"}),
            add("ch02_help","Saksi mencari bantuan",new[]{"talk_arga|Bicarakan ketakutanmu dengan Arga","ask_teacher|Sampaikan fakta kepada Bu Nadia"}),
            add("ch02_support","Jaga persetujuan dan privasi",new[]{"share_witness|Tanyakan batas informasi kepada Dimas","clarify_boundaries|Sepakati bantuan aman bersama Arga"}),
            add("ch02_reflection","Refleksi: pilihan saksi",new[]{"reflect|Selesaikan dua pertanyaan refleksi"})});
        chapters.Add(new[]{
            add("ch03_listen","Tawa tidak berarti setuju",new[]{"listen_member|Dengarkan Bima di lapangan"}),
            add("ch03_accountability","Alasan tidak menghapus dampak",new[]{"own_actions|Akui tindakanmu kepada Bu Nadia"}),
            add("ch03_repair","Perbaikan dengan batas",new[]{"ask_consent|Tanyakan batas yang Dimas inginkan","make_plan|Buat langkah tanggung jawab bersama Bu Nadia"}),
            add("ch03_reflection","Refleksi: bertanggung jawab",new[]{"reflect|Selesaikan dua pertanyaan refleksi"})});
        chapters.Add(new[]{
            add("ch04_check","Tidak perlu menyalahkan diri",new[]{"park_check|Bicarakan perasaanmu dengan Dimas"}),
            add("ch04_support","Bukti bukan tontonan",new[]{"preserve_evidence|Minta Nara membantu menjaga bukti","seek_support|Minta pendampingan Bu Nadia"}),
            add("ch04_boundaries","Berlatih menyatakan batas",new[]{"state_boundary|Sepakati langkah aman bersama Dimas"}),
            add("ch04_reflection","Refleksi: hak atas rasa aman",new[]{"reflect|Selesaikan dua pertanyaan refleksi"})});
        chapters.Add(new[]{
            add("ch05_facts","Saksi yang dapat dipercaya",new[]{"verify_facts|Pisahkan fakta dari rumor bersama Arga"}),
            add("ch05_consent","Pertemuan dengan persetujuan",new[]{"confirm_boundaries|Periksa kembali batas Dimas"}),
            add("ch05_action","Janji harus menjadi tindakan",new[]{"check_commitment|Tanyakan tindak lanjut Raka","follow_up|Susun pemeriksaan berikutnya bersama Bu Nadia"}),
            add("ch05_reflection","Refleksi: lingkungan yang berubah",new[]{"reflect|Selesaikan dua pertanyaan refleksi"})});
        Chapter(1,"Hari yang tidak biasa • Halaman sekolah","Arga",POVType.Victim,"Opening_Victim","Arga dan Dimas memperoleh pendampingan. Bantuan dan batas mulai menggantikan rasa terisolasi.",chapters[0],
            "Saat teman dipalak, bantuan awal yang aman adalah…|Menyalahkannya karena takut|Menemani dan menghubungi pihak tepercaya|Memaksanya melawan sendiri|1|Korban tidak wajib menghadapi pelaku sendirian. Dengarkan, dampingi, dan cari bantuan yang aman.",
            "Kalah pada QTE berarti…|Korban pantas dirundung|Korban dilarang meminta bantuan|Tantangan belum berhasil, hak atas dukungan tetap ada|2|Hasil QTE tidak menentukan kesalahan atau nilai diri korban. Cerita dan kesempatan mencari dukungan tetap berlanjut.");
        Chapter(2,"Diam yang berat • Taman sekolah","Nara",POVType.Witness,"Opening_Witness","Nara memilih mendengarkan, menjaga privasi, dan meminta bantuan. Menjadi saksi berarti bertindak dengan memperhatikan keselamatan.",chapters[1],
            "Jika takut ikut menjadi sasaran, saksi dapat…|Mencari pendamping dan bantuan yang aman|Merekam untuk hiburan|Mengabaikan selamanya|0|Bantuan tidak harus berupa konfrontasi langsung. Pendampingan dan laporan faktual adalah tindakan yang berarti.",
            "Sebelum membagikan cerita korban, sebaiknya…|Mengirim ke seluruh grup|Meminta persetujuan dan membatasi informasi|Menambah dugaan agar dipercaya|1|Hormati persetujuan dan privasi. Sampaikan fakta kepada pihak yang membantu penanganan.");
        Chapter(3,"Kuat di mata siapa? • Lapangan","Raka",POVType.Bully,"Opening_Bully","Raka mengenali tekanan yang ia alami dan dampak yang ia timbulkan. Ia harus menghentikan tindakan dan membuktikan perbaikan tanpa menuntut maaf.",chapters[2],
            "Tekanan yang Raka alami di rumah…|Membenarkan ia mengancam orang lain|Menghapus tanggung jawabnya|Perlu ditangani, tetapi tidak membenarkan perundungan|2|Latar menjelaskan, tidak membenarkan. Pelaku tetap perlu menghentikan tindakan dan bertanggung jawab atas dampaknya.",
            "Perbaikan yang menghormati korban adalah…|Memaksa korban menerima maaf|Mengikuti batas korban dan langkah pendampingan|Menuntut korban melupakan kejadian|1|Korban tidak wajib memaafkan. Perubahan ditunjukkan melalui tindakan dan penghormatan atas batas.");
        Chapter(4,"Berani menetapkan batas • Taman lingkungan","Arga",POVType.Victim,"Opening_Boundaries","Arga menjaga bukti, mencari pendampingan, dan menyatakan batas. Pengalaman takut tidak mengurangi haknya untuk aman.",chapters[3],
            "Ejekan berulang di grup sebaiknya ditangani dengan…|Menyimpan bukti secara terbatas dan meminta bantuan|Mengirim ulang untuk mempermalukan semua pihak|Menyalahkan diri karena tidak cukup kuat|0|Simpan bukti tanpa menyebarkan ulang perundungan. Gunakan dukungan dan fitur laporan yang aman.",
            "Korban sulit menolak saat diancam karena…|Ia setuju atas perlakuan itu|Tekanan dan ketimpangan kuasa dapat membatasi pilihan|Ia pantas dijadikan sasaran|1|Diam, menangis, atau mengikuti tuntutan di bawah ancaman bukan persetujuan dan bukan kesalahan korban.");
        Chapter(5,"Langkah yang dipilih bersama • Halaman sore","Nara",POVType.Witness,"Opening_Together","Lima bab berakhir dengan rencana tindak lanjut: lindungi korban, hormati batas, hentikan dukungan pada perundungan, dan periksa tindakan pelaku.",chapters[4],
            "Sesudah pelaku berjanji berhenti, langkah berikutnya…|Menganggap semua dampak hilang|Meminta korban segera berteman dengannya|Memeriksa tindakan dan menindaklanjuti keselamatan korban|2|Janji perlu tindakan yang dapat diperiksa. Dukungan korban tetap dibutuhkan setelah pertemuan.",
            "Keterangan saksi yang dapat dipercaya berisi…|Fakta yang dilihat, dengan menjaga privasi|Rumor yang paling menarik|Video korban untuk hiburan|0|Pisahkan fakta dari dugaan. Jaga privasi dan persetujuan saat membantu penanganan.");
        AssetDatabase.SaveAssets();return "Saved five outdoor chapters, 22 prerequisite-linked missions, 10 reflections and purposeful character/backstory dialogue.";
    }
}
