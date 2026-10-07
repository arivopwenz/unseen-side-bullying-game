# Chapter 1–2 — Aji, Denis, Ari, Billy dan Pak Bambang

> **Revisi tampilan dan scene terbaru:** [Presentation_SceneSplit_Revision.md](Presentation_SceneSplit_Revision.md). Setiap chapter sekarang mempunyai Level01–Level05 sendiri; UI/kamera/pembukaan dan pertanyaan quest telah diperbarui. Laporan di bawah merupakan baseline cerita sebelum revisi tampilan tersebut.

Pembaruan 7 Oktober 2026. Arahan pengguna pada revisi ini menggantikan rancangan Chapter 1 POV Arga / Chapter 2 POV Nara sebelumnya. Pengguna mengizinkan authoring langsung di Unity dan memilih ruang kelas **indoor khusus** untuk adegan papan tulis. Halaman, kantin, perjalanan dan mayoritas lokasi tetap outdoor. Model karakter final disediakan pengguna pada tahap berikutnya.

## Tujuan pengalaman

Chapter 1 memperkenalkan Aji sebagai protagonis dan jembatan antara karakter. Ia belum tahu seluruh persoalan; cara menanggapi Denis memengaruhi percakapan dan langkah berikutnya. Chapter 2 mengganti POV menjadi Denis. Pemain mengalami konsentrasi yang terganggu, ejekan terlepas dari hasil pelajaran, pemalakan makanan dan uang, ancaman yang membuat mencari bantuan terasa menakutkan, serta awal kehadiran Ari.

Nama sementara: Aji (pengantar/protagonis), Denis (korban), Ari (saksi), Billy (pemimpin pelaku), Pak Bambang (guru). Bima/Fajar tetap anggota kelompok prototipe; Bu Sari adalah penjual kantin sementara. Pengalaman tekanan merupakan pengalaman karakter fiksi, bukan diagnosis atau ukuran nilai diri pemain. Latar pelaku tidak membenarkan tindakannya.

## Chapter 1 — Sebelum memahami

1. **Pembukaan Timeline.** Aji melihat Billy menghalangi Denis dan Ari yang takut bertindak. Signal membuka percakapan; Timeline berhenti sementara teks dibaca. Setelah selesai/dilewati, kontrol kembali dan objective menemui Denis muncul.
2. **Temui Denis: dengarkan sebelum bertindak.** Denis menyatakan batas privasi dan menjelaskan buku tugasnya hilang. Pemain memilih satu dari tiga respons Aji.
3. **Jalur respons Aji.** Satu jalur dipilih; dua jalur lain tidak ikut aktif.
4. **Buku tugas Denis yang hilang.** Denis adalah pemilik dan penerima buku. Ada alasan konkret membantu: tugas yang harus dikumpulkan dan catatan untuk mengejar pelajaran. Identitas quest lama `quest_level01_lost_notebook` serta objective `accept_request`, `find_notebook`, `return_notebook` dipertahankan. Timeline encounter/QTE lama tetap digunakan. QTE gagal memindahkan item yang sama ke anchor berbeda yang dapat dicapai; pengambilan ulang tidak mengulang encounter. Menemukan buku belum menyelesaikan quest: kembalikan kepada Denis.
5. **Jangan hadapi pungli sendirian.** Dengarkan Denis, kemudian tanyakan pendampingan kepada Pak Bambang. Aji tidak menjanjikan bahwa satu laporan langsung menghapus rasa takut atau risiko.
6. **Temani ke area aman.** Minta persetujuan, berjalan bersama ke lingkaran hijau, tunggu keduanya tiba, lalu periksa keadaan Denis. Dialog menyebut besok masih dapat terasa menakutkan.
7. **Dukungan dan saksi.** Ari mengungkap ketakutannya. Percakapan dengan Pak Bambang berisi tiga respons Aji tentang tindak lanjut; guru menanggapi serta mengoreksi rencana yang meningkatkan risiko.
8. **Dua refleksi dan penutup.** Bantuan, batas dan pendampingan mulai terbentuk. Masalah tidak dinyatakan selesai secara ajaib. Berikutnya pemain menjadi Denis.

### Tiga jalur pertama

| Respons | Langkah yang benar-benar berubah | Dampak percakapan berikutnya |
|---|---|---|
| Mendengarkan dan menawarkan bantuan dengan persetujuan | Bicara lagi kepada Denis tentang lokasi buku dan batas bantuan | Pembukaan Chapter 2 mengingat kehadiran Aji yang mendengarkan |
| Memanas-manasi: mempermalukan Billy di depan orang lain | Konfrontasi verbal dengan Billy, lalu kembali ke Denis dan memperbaiki pendekatan | Denis menyatakan konfrontasi menambah kekhawatiran meski Aji sudah meminta maaf |
| Mengabaikan: menyuruh Denis menyelesaikan sendiri | Dengarkan Ari, lalu kembali kepada Denis dan tawarkan bantuan | Denis mengingat Aji sempat meninggalkannya; kepercayaan memerlukan waktu |

Semua jalur tetap memberikan kesempatan melanjutkan cerita. Tidak ada adegan pertarungan sebagai hadiah; pilihan mengajak berkelahi ada pada tindak lanjut dan mendapat respons guru tentang risiko serta alternatif tindakan. Pilihan pertama/ketiga pada dua percakapan tersebut memenuhi kategori baik, provokatif, dan acuh/konfrontatif yang diminta pengguna tanpa menggabungkan acuh dan berkelahi menjadi satu tindakan ambigu.

Pilihan hanya tersedia untuk Aji pada chapter yang mengizinkannya (`AllowsDialogueChoices` dan `POVType.Protagonist`). Denis, Billy, dan Ari tidak menampilkan menu tiga respons ini. Konsekuensi saat ini berupa rute objective, respons karakter, dan ingatan di pembukaan berikutnya. Belum merupakan sistem kepercayaan numerik atau banyak ending.

## Chapter 2 — Hari yang terasa panjang

| Misi | Yang dimainkan | Fungsi cerita |
|---|---|---|
| Belajar di bawah tekanan | Datangi Pak Bambang di ruang kelas; pilih jawaban `7 × 6 + 8` di panel papan tulis tanpa timer | Jawaban benar maupun salah diikuti ejekan Billy. Guru menegur dan menjelaskan belajar tanpa mempermalukan. Tidak ada penalti akademik/poin pendidikan |
| Istirahat yang tidak tenang | Beli makanan kepada Bu Sari; kelompok Billy mendatangi Denis; QTE usaha memakai Input System yang sama | **Makanan direbut pada kedua hasil.** Usaha berhasil dan tubuh membeku mempunyai respons berbeda; keduanya tidak menyalahkan Denis |
| Ancaman yang membungkam | Temui Billy di halaman; dengarkan pemalakan uang dan ancaman bila mengadu | Denis menjelaskan takut kepada pembalasan dan belum sanggup bercerita kepada guru/orang tua; vignette dan bunyi tekanan menggambarkan adegan |
| Seseorang melihat | Temui Ari di dekat bangku | Ari mengakui takut, menjaga privasi, menawarkan kehadiran tanpa memaksa Denis langsung mengadu |
| Refleksi: memahami tekanan Denis | Dua pertanyaan dengan penjelasan | Hasil berhitung dan ketakutan tidak membenarkan perundungan; pendampingan perlu aman dan berlanjut |

Adegan sekolah sebelumnya berfungsi sebagai pengalaman emosional; belum ada sistem simulasi nilai akademik per semester. Penurunan konsentrasi, kurang tidur dan kekhawatiran disampaikan lewat dialog cinematic. Tindak lanjut saksi serta penanganan sekolah perlu dikembangkan pada chapter berikutnya.

## Area, kamera dan kenyamanan

- Ground halaman menjadi **64 × 56 meter**, dari sebelumnya sekitar 28 × 28. Perimeter collider dan NavMesh ikut diperbesar. Jangan hanya memperbesar visual lantai: pembatas lama dapat tetap menghalangi akses.
- Ruang kelas berada di sisi barat laut halaman, sekitar `(-20, 0, 15)`, ukuran 12 × 10 meter, dengan atap, dinding dan pintu depan. Meja/kursi prototipe belum collider agar jalur belajar mudah diakses. Papan tulis berada di dinding belakang. Kamera gameplay khusus aktif di dalam kelas; Cinemachine tetap mengendalikan Main Camera.
- Kantin berada sekitar `(18, 0, 6.5)`, dengan counter, kanopi, dan properti makanan. Penjual masih bentuk prototipe sederhana; tidak diklaim sebagai model final.
- Posisi NPC ditentukan oleh `ChapterActorLayout`; classroom shot dan gang staging ditangani `NarrativeActivityDirector`. Group pose dan kontrol dipulihkan setelah adegan.
- Petunjuk navigasi menunjukkan target, jarak dan arah; petunjuk belajar kontekstual menjelaskan tugas serta cara kerja encounter. Petunjuk disembunyikan selama modal/cinematic.
- Dialog, tiga respons Aji dan jawaban matematika memakai TMP/UI serta EventSystem. Navigasi UI dapat dipakai keyboard/gamepad, mouse atau sentuh. Soal tidak memiliki timer.
- Vignette tekanan **statis dan memudar**, tanpa flashing atau camera shake. Bunyi tekanan prototipe orisinal volumenya rendah. Keduanya dapat dimatikan. Tidak ada label QTE "gagal" yang menyalahkan Denis atau bunyi hadiah saat makanan tetap dirampas.
- Menu utama dan Pause menyediakan teks dialog langsung, mematikan efek/bunyi tekanan, dan bantuan QTE pemalakan makanan. Bantuan melewati usaha QTE lalu tetap menjalankan aftermath naratif. Ini tidak mengubah hasil encounter notebook Aji.

## Pemisahan sistem

- `DialogueData` menyimpan tiga respons, ID pilihan dan respons kontekstual. `DialogueManager` mempertahankan identitas percakapan asal saat memainkan respons, supaya pemilik percakapan/cancel tetap benar.
- `DialogueChoiceUI` menampilkan pilihan. Pilihan baru dicatat saat respons selesai normal. Membatalkan tidak menyelesaikan objective maupun merekam pilihan.
- Quest route memiliki ID pilihan dan indeks respons yang membuatnya berlaku. `StoryChapterSequence` melewati route lain; validasi save/prasyarat hanya menuntut route yang berlaku.
- `NarrativeActivityData` menyimpan soal atau QTE usaha serta dua aftermath. `NarrativeActivityDirector` memiliki snapshot kontrol, camera priority dan posisi kelompok. Progress objective diberikan setelah aftermath selesai normal, bukan sebelum pertanyaan/event.
- Matematika memakai modal `GameState.Quiz` tetapi **tidak memanggil QuizManager**. Dengan demikian ia tidak memajukan refleksi chapter atau memberi poin empati. Refleksi memakai QuizManager seperti sebelumnya.
- Save format 3 menambahkan array pilihan bernama `narrativeChoices` dan tetap membaca save format sebelumnya. ID misi baru menghindari menyamakan progress POV saksi lama dengan pengalaman Denis baru. Save lama dapat kembali ke chapter pertama yang prasyarat barunya belum lengkap; save pemain tidak direset untuk memasang revisi.
- Lima chapter tetap terdaftar. Ada **26 definisi quest**, termasuk tiga route yang saling eksklusif; **24 quest dimainkan** dalam satu perjalanan campaign. Chapter 3–5 tetap prototipe lama yang namanya disesuaikan; penulisan mendalam pada revisi ini fokus Chapter 1–2.

## Authoring dan pengujian

Authoring native Editor: `Tools/UnityAutomation/Chapter12Authoring.cs` untuk data, `Chapter12Scene.cs` untuk scene/menu. Keduanya berjalan melalui `unity cmd run_script`; scene, ScriptableObject, material, sprite dan NavMesh tidak diedit melalui YAML mentah. Jalankan pada Edit Mode dan gunakan satu Editor saja.

Pengujian: `Tools/UnityAutomation/Chapter12Smoke.cs`. Slot fixture berada di `Logs/AutomationChapter12/savegame.json`; screenshot berada di folder yang sama. Pengujian lama dengan hitungan 22 atau nama/path Arga/Dimas/Nara merupakan bukti konfigurasi sebelumnya dan tidak otomatis berlaku pada revisi ini.

Hasil Play Mode / Edit Mode revisi:

| Pemeriksaan | Hasil |
|---|---|
| `Integrity` | 800 cek lulus: lima chapter, 26 definisi valid/unik, hanya Aji memiliki pilihan, ground 64 × 56, jalur kelas/kantin/NPC lengkap, tidak ada missing script |
| `SupportRoute` | 32 cek lulus: respons baik, pembatalan tidak memberi progress/pilihan, permintaan Denis, checkpoint route |
| `ProvokeRoute` | 35 cek lulus: konfrontasi verbal Billy → memperbaiki pendekatan kepada Denis → buku, save/load |
| `IgnoreRoute` | 35 cek lulus: Ari → kembali kepada Denis → buku, save/load |
| `FinishChapterOne` | 36 cek lulus: encounter nyata, QTE timeout, random rehide, recollect tanpa replay, turn-in Denis, escort NavMesh, dialog/reflect dan transisi POV Denis |
| `DenisCorrectEffort` | 50 cek lulus: tombol jawaban UI, pause, jawaban benar tetap diejek, input gamepad nyata menyelesaikan usaha QTE, makanan tetap dirampas, ancaman/Ari/reflect dan kontrol pulih |
| `DenisWrongTimeout` | 50 cek lulus: salah menjawab tetap berlanjut, QTE timeout memiliki aftermath berbeda, objective/reflect/control tetap benar |
| `ActivityGuards` | 32 cek lulus: cancel saat pause, cancel aftermath, retry, tidak memberi objective dini, bantuan QTE, mematikan vignette/bunyi, preference fixture dikembalikan |
| `RemainingCampaign` | 56 cek lulus: Chapter 3–5 tetap dapat diselesaikan, hasil akhir dan pilihan tersimpan, reload tidak menampilkan refleksi lama di atas hasil |
| `ResumeCampaignAndMenu` | 18 cek lulus: tombol menu/settings/new game, trigger Denis nyata, gamepad interaksi dan konfirmasi pilihan UI |
| `Chapter12MenuCheck.Run` | 9 cek lulus setelah perbaikan layout: panel pengaturan terbuka, tiga toggle, empat sudut berada di layar, screenshot, binding tombol tutup dan tombol benar-benar menutup panel; preference/save pemain tidak diubah oleh pemeriksaan ini |

Fixture menu/QTE menggunakan Input System gamepad agar tidak bergantung pada fokus keyboard desktop. Pemeriksaan awal dengan keyboard simulasi gagal menerima semua tap saat Editor berada di latar; pengujian ulang dengan perangkat gamepad sintetis menghasilkan usaha yang berhasil. Fixture bantuan QTE berhenti pada identitas percakapan asal agar tidak ikut menyelesaikan aftermath yang dibuka pada frame yang sama. Fixture chapter berikutnya menunggu refleksi yang masih dijadwalkan. Kegagalan fixture tersebut diperbaiki dan pengujian ulang lulus; tidak disembunyikan sebagai hasil sukses awal.

Screenshot visual meliputi pilihan Aji melalui UI, kelas/papan tulis, QTE makanan, aftermath, hasil chapter, menu dan pengaturan Pause. Blocking makanan dipindahkan ke depan kantin supaya kelompok tidak berdiri di counter. Properti makanan terlihat pada Denis lalu Billy selama aftermath. Pengaturan memiliki penanda checkbox saat off/on.

Pemeriksaan visual terakhir menemukan panel pengaturan menu masih mewarisi anchor di luar layar. Anchor panel dipusatkan, slider volume dirapikan, tombol kembali diberi binding native, dan teks pengantar memakai nama karakter terbaru. Screenshot `Logs/AutomationChapter12/MenuSettings.png` diperbarui setelah perbaikan dan diperiksa secara visual pada 1920 × 1080.

Build Windows revisi terakhir **berhasil** pada 7 Oktober 2026, build ID `build_3fc6dee8bda3`: **0 error, 3 warning**, ukuran laporan 125.369.335 byte dan waktu native build sekitar 61 detik. Output berada di `Builds/WindowsPrototype/TheUnseenSide.exe`; seluruh folder pendamping harus tetap bersama executable. Laporan lengkap: `Logs/Chapter12WindowsBuildReport.json`.

Tiga warning adalah server otomasi Pipeline yang sengaja dinonaktifkan pada Player build (`enableInBuilds = false`) dan stripping shader debug bawaan `Hidden/Core/DebugOccluder` / `Hidden/Core/DebugOcclusionTest`. Tidak ada compile error C# atau shader gameplay dalam laporan. Pemeriksaan Console native terakhir menunjukkan compilationFailed false, 0 error dan 3 warning build tersebut; riwayat transport CLI lama bukan error gameplay baru.

Executable terbaru diuji startup dengan `-batchmode -nographics`: log mencapai `Boot → Loading → MainMenu`, tanpa exception yang terdeteksi. Pemeriksaan awal 12 detik hanya mencapai Loading; pemeriksaan ulang dengan batas 45 detik mencapai MainMenu. Ini memverifikasi startup build, bukan tampilan grafis Windows; tampilan UI/kelas/QTE diperiksa melalui screenshot Play Mode Editor. Log: `Logs/Chapter12PlayerStartup.log`. Proses uji ditutup setelah pemeriksaan, dan Editor yang sudah ada dikembalikan ke Bootstrap Edit Mode. Editor kedua tidak dibuka.

## Cara mencoba

1. Buka Bootstrap, tekan Play, lalu pilih **Mulai baru** bila ingin melihat alur baru dari awal. Tombol ini memang mengganti checkpoint pemain; gunakan Lanjutkan untuk mempertahankan progress.
2. Selesaikan pembukaan atau gunakan Lewati. Temui Denis dan baca seluruh percakapan sebelum memilih respons Aji.
3. Ikuti jalur yang muncul. Misi buku sekarang diminta serta diselesaikan kepada Denis.
4. Selesaikan pendampingan, Ari dan refleksi; lanjutkan chapter untuk menjadi Denis.
5. Datangi kelas di barat laut. Setelah soal dan aftermath, ikuti target kantin, Billy, lalu Ari.
6. Gunakan Pause untuk pengaturan kenyamanan. Untuk membandingkan ketiga jalur dan kedua hasil aktivitas, gunakan sesi baru/slot pengujian terpisah; tidak perlu mengubah scene manual.

## Pekerjaan berikutnya

Model humanoid dan animasi final mengikuti aset pengguna. Setelah alur revisi diuji, lakukan playtest manusia untuk tempo dialog, kenyamanan kamera, navigasi dalam area luas dan pemahaman pesan cerita. Tidak menjanjikan "bebas semua bug" atau final art. Cabang ending, voice-over final, video After Effects, profiling target dan Android fisik tetap pekerjaan terpisah.
