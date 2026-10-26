using System.Collections.Generic;
using UnityEngine;

namespace BullyingGame.Core
{
    // Voice untuk subtitle: dipanggil driver subtitle tiap baris tampil.
    // PlayLine(line) memutar klip yang dipetakan ke baris itu (dari
    // Resources/Voice) dan mengembalikan durasi klip (>0) agar driver bisa
    // menahan subtitle selama suara berjalan; -1 bila tidak ada klip
    // (driver tetap memakai lineDisplayTime). Satu channel suara: baris
    // baru selalu menghentikan klip sebelumnya. Klip kosong = diam.
    public class VoiceLinePlayer : MonoBehaviour
    {
        private static VoiceLinePlayer instance;
        private AudioSource source;
        private Dictionary<string, string> map;

        // Sakelar nonaktif sementara (Ari, 2026-10-26): false = semua dialog
        // bisu; subtitle tetap jalan dengan pacing 4 detik seperti sebelum
        // voice. Mengaktifkan lagi: ubah ke true. Tidak ada file yang dihapus.
        public static bool Enabled = false;
        public static string LastClipName { get; private set; } = "";
        public static bool IsPlaying => instance != null && instance.source != null && instance.source.isPlaying;

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;
            source = GetComponent<AudioSource>();
            if (source == null) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            BuildMap();
        }

        private void BuildMap()
        {
            map = new Dictionary<string, string>();
            // Walk-talk (WP_01..WP_05)
            map["Bima: Eh, kamu anak baru ya? Aku Bima. Sini, aku anterin ke kelas."] = "wt01_bima";
            map["Aji: Iya, hari pertama. Makasih ya, Bima."] = "wt02_aji";
            map["Bima: Itu Gedung Piala — piala-piala sekolah dipajang di sana."] = "wt03_bima";
            map["Bima: Nanti kalau gedungnya sudah jadi, kita lewat pintu tengahnya. Sekarang muter dulu, ya."] = "wt04_bima";
            map["Bima: Ini lapangan. Istirahat nanti biasanya rame di sini."] = "wt05_bima";
            map["Bima: Lewat koridor ini, kelas kita sudah dekat."] = "wt06_bima";
            map["Bima: Nah, ini kelas kita. Masuk yuk — nanti aku kenalin ke teman-teman."] = "wt07_bima";
            // Ambush
            map["Nakal: Eh, lihat siapa ini. Si gendut bawa anak baru, sok jadi pemandu."] = "amb01_nakal";
            map["Nakal1: Jalan aja ngos-ngosan, masih sempat sok akrab."] = "amb02_nakal1";
            map["Bima: Maaf... kami cuma mau lewat ke kelas."] = "amb03_bima";
            map["Nakal: Kelas? Tempat kamu itu di kantin, bukan di kelas. Minggir sana!"] = "amb04_nakal";
            map["Nakal1: Anak baru, hati-hati ya. Ketularan leletnya nanti."] = "amb05_nakal1";
            // Kelas (baris 0 = bel, bukan dialog)
            map["[Bel sekolah berbunyi]"] = "bell";
            map["Aji: Selamat pagi, semuanya. Nama saya Aji, murid baru. Senang bertemu kalian."] = "cls02_aji";
            map["Ibu Guru: Selamat datang di kelas ini, Aji. Semoga cepat betah ya."] = "cls03_guru";
            map["Nakal: (dari pintu) Eh, anak baru sok percaya diri. Tadi aja dibuntuti si gendut."] = "cls04_nakal";
            map["Nakal1: Hah, perkenalan macam apa itu. Duduk sana, jangan banyak gaya."] = "cls05_nakal1";
            map["Bima: (pelan) Udah, Ji... jangan diladenin."] = "cls06_bima";
            // Misi 2 intro + insiden + penutup
            map["Sari: (narasi) Bel istirahat berbunyi. Sekarang giliranmu mengamati, Ji — tiga kejadian, tanpa ikut menertawakan."] = "m2in01_sari";
            map["Sari: (narasi) Mulai dari lapangan. Bima ada di sana."] = "m2in02_sari";
            map["Nakal: (menunjuk) Eh, itu si gendut lagi. Lapangan bukan tempat buat yang jalannya lelet."] = "m2i101_nakal";
            map["Nakal1: Iya, istirahat aja ngos-ngosan. Pulang sana, jangan bikin malu."] = "m2i102_nakal1";
            map["Bima: (menunduk) Aku cuma mau duduk di sini sebentar..."] = "m2i103_bima";
            map["Sari: (narasi) Lihat itu, Ji. Kamu tidak ikut menertawakan — itu sudah benar."] = "m2i104_sari";
            map["Nakal: Eh, kelompok kami sudah penuh. Cari kelompok lain aja ya."] = "m2i201_nakal";
            map["Nerd: Oh... oke. Tidak apa-apa, aku belajar sendiri saja."] = "m2i202_nerd";
            map["Sari: (narasi) Penolakan seperti itu sakitnya tidak kelihatan, Ji."] = "m2i203_sari";
            map["Nakal: Buku tugasmu sini. Aku yang kumpulin, atas nama aku."] = "m2i301_nakal";
            map["Nerd: (tegang) Tapi... itu tugasku. Aku yang kerjakan semalam."] = "m2i302_nerd";
            map["Nakal: Makanya jangan banyak protes. Besok bawa lagi yang baru."] = "m2i303_nakal";
            map["Sari: (narasi) Itu bukan pinjam, Ji. Itu pemerasan."] = "m2i304_sari";
            map["Sari: (narasi) Tiga kejadian sudah kamu amati. Polanya jelas sekarang: Bima dan dia selalu jadi sasaran."] = "m2cl01_sari";
            map["Sari: (narasi) Sekarang, uluran tangan ringan. Dekati mereka — sapa, tawarkan bantuan."] = "m2cl02_sari";
            // Misi 3 dukungan
            map["Aji: Hai, Bima. Tadi di lapangan... kamu tidak apa-apa?"] = "m3s101_aji";
            map["Bima: (kaget) Kamu... tidak ikut menertawakan aku. Makasih, Ji."] = "m3s102_bima";
            map["Aji: Kalau mau cerita, aku dengerin. Tidak usah buru-buru."] = "m3s103_aji";
            map["Aji: Hai. Kalau diskusi kelompok lagi, boleh aku gabung sama kamu?"] = "m3s201_aji";
            map["Nerd: (ragu) Kamu serius? Biasanya orang cuma lewat saja..."] = "m3s202_nerd";
            map["Aji: Serius. Kita belajar bareng, tidak usah peduliin mereka."] = "m3s203_aji";
            map["Aji: Bukumu kemarin diambil ya? Ini, pakai pulpenku dulu."] = "m3s301_aji";
            map["Nerd: Makasih... belum pernah ada yang nawarin begitu."] = "m3s302_nerd";
            map["Aji: Kalau dia minta lagi, bilang aku. Kita hadapi bareng."] = "m3s303_aji";
            map["Sari: (narasi) Mereka mulai percaya padamu, Ji. Itu langkah pertama — mereka siap cerita di lain waktu."] = "m3cl01_sari";
        }

        public static float PlayLine(string line)
        {
            if (!Enabled) return -1f;
            if (instance == null)
            {
                var go = new GameObject("VoiceLinePlayer");
                instance = go.AddComponent<VoiceLinePlayer>();
            }
            return instance.Play(line);
        }

        private float Play(string line)
        {
            LastClipName = "";
            if (source == null || line == null || map == null) return -1f;
            if (!map.TryGetValue(line, out string clipName)) return -1f;
            var clip = Resources.Load<AudioClip>("Voice/" + clipName);
            if (clip == null) return -1f;
            source.Stop();
            source.clip = clip;
            source.Play();
            LastClipName = clipName;
            return clip.length;
        }

        public static void StopVoice()
        {
            if (instance != null && instance.source != null) instance.source.Stop();
            LastClipName = "";
        }
    }
}
