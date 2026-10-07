# The Unseen Side — Implementasi Lima Chapter Outdoor

> **Routing/tampilan terbaru:** [Presentation_SceneSplit_Revision.md](Presentation_SceneSplit_Revision.md). Level01 hanya untuk Chapter 1; Level02–Level05 memiliki host chapter terpisah. HUD bersih dan perkenalan karakter memakai Timeline.

> **Revisi cerita terbaru:** baca [Chapter01_02_Narrative_Revision.md](Chapter01_02_Narrative_Revision.md). Chapter 1 sekarang memakai Aji dengan tiga jalur dialog, Chapter 2 memakai Denis dengan miniquest berhitung, pemalakan makanan, ancaman dan Ari sebagai saksi. Billy/Pak Bambang menggantikan nama Raka/Bu Nadia. Pengguna memilih kelas indoor khusus; lokasi lainnya tetap dominan outdoor. Ada 26 definisi quest, 24 dimainkan per route. Rincian cerita dan angka pengujian di bawah adalah riwayat prototipe sebelumnya, bukan bukti otomatis untuk revisi baru.

Pembaruan: 7 Oktober 2026. Dokumen ini menggantikan arahan lama bahwa pengguna harus memasang seluruh konfigurasi Unity secara manual. Pengguna sudah memberi izin kepada Codex untuk mengerjakan kode, scene, aset data, kamera, Timeline dan UI langsung melalui Editor.

## Batas operasional

- Gunakan satu Editor Unity yang sudah terbuka. RAM mesin sekitar 6 GB. Proses AssetImportWorker merupakan anak Editor, bukan Editor proyek kedua; jumlah pekerja impor telah dibatasi menjadi satu melalui `AssetDatabase.DesiredWorkerCount`.
- Jangan mengedit YAML scene/prefab secara langsung. Seluruh authoring scene di pekerjaan ini menggunakan API Editor, SerializedObject, AssetDatabase dan EditorSceneManager.
- Simpan kode otomasi di `Tools/UnityAutomation`, bukan `Temp`, karena Unity dapat membersihkan Temp ketika restart.
- Jangan menimpa save pemain untuk pengujian. Skenario Play Mode menggunakan `Logs/AutomationCampaign/savegame.json` setelah mengganti path instance SaveManager khusus sesi pengujian.
- Jangan menyatakan bebas seluruh bug atau siap rilis hanya karena kompilasi lulus. Bedakan validasi prototipe, build, media final dan perangkat fisik.

## Arah cerita yang sudah diterapkan

Ada lima chapter, semuanya di ruang terbuka. Satu scene `Level01` dipakai bersama untuk mengurangi pemuatan dan penggunaan memori. Set dekorasi, material lantai dan pencahayaan berubah mengikuti chapter; lokasi masih berupa lingkungan prototipe, bukan lima peta final yang terpisah.

| Chapter | POV utama | Lokasi | Konflik dan perkembangan |
|---|---|---|---|
| 1 — Hari yang tidak biasa | Arga, korban | Halaman sekolah | Arga murid baru yang ingin diterima. Ia membantu mencari buku Raka, mengalami intimidasi, mendampingi Dimas yang dipalak, kemudian mencari dukungan. |
| 2 — Diam yang berat | Nara, saksi | Taman sekolah | Nara takut dikucilkan setelah melihat orang yang membela korban ikut diejek. Ia mulai menemani, menyampaikan fakta dan menjaga persetujuan korban. |
| 3 — Kuat di mata siapa? | Raka, pelaku | Lapangan sekolah | Tekanan perbandingan di rumah dan kebutuhan dianggap kuat dijelaskan melalui cutscene. Raka harus mengakui dampak, menghentikan tindakan dan mengikuti batas korban. |
| 4 — Berani menetapkan batas | Arga, korban | Taman lingkungan | Arga menghadapi ejekan berulang di grup, menjaga bukti secara terbatas, mencari pendampingan dan menyatakan batas yang aman. |
| 5 — Langkah yang dipilih bersama | Nara, saksi | Halaman sekolah sore | Para karakter memeriksa fakta, persetujuan dan tindakan pelaku. Pertemuan berakhir dengan rencana tindak lanjut, bukan tuntutan agar korban segera memaafkan. |

Arga adalah karakter utama POV korban. Dimas adalah teman laki-laki yang juga menjadi korban. Raka adalah pemimpin kelompok perundung. Bima dan Fajar adalah anggota kelompok. Nara adalah saksi yang kelak dimainkan. Bu Nadia adalah pendamping dewasa; perannya berbeda dari Dimas. Nama-nama ini dapat diganti tanpa mengganti mekanik.

Kesulitan menolak, diam atau menangis menggambarkan tekanan dan ketimpangan kuasa. Hal tersebut tidak menyebabkan korban pantas dirundung. Tekanan yang dialami Raka menjelaskan latar, tetapi tidak membenarkan pemalakan atau intimidasi. Pengampunan dan kedekatan dengan pelaku tidak menjadi syarat menyelesaikan misi.

## Dua puluh dua misi

| Chapter | Misi | Pelaksanaan |
|---|---|---|
| 1 | Dengarkan Dimas | Percakapan awal Dimas; percakapan yang dibatalkan tidak menyelesaikan tujuan. |
| 1 | Buku catatan dan batas | Dengarkan permintaan Raka → cari buku → encounter Timeline/dialog/QTE → kembalikan buku kepada Raka. |
| 1 | Jangan hadapi pungli sendirian | Dengarkan ancaman yang Dimas alami → minta bantuan Bu Nadia. |
| 1 | Temani ke area aman | Minta persetujuan Dimas → berjalan bersama menuju lingkaran hijau → periksa keadaannya setelah tiba. |
| 1 | Dukungan dan saksi | Dengarkan Nara → susun tindak lanjut bersama Bu Nadia. |
| 1 | Refleksi: dukungan yang aman | Dua pertanyaan dengan penjelasan. |
| 2 | Dari diam menjadi hadir | Minta izin menemani Dimas. |
| 2 | Saksi mencari bantuan | Bicarakan ketakutan dengan Arga → sampaikan fakta kepada Bu Nadia. |
| 2 | Jaga persetujuan dan privasi | Tanyakan batas informasi kepada Dimas → sepakati bantuan aman bersama Arga. |
| 2 | Refleksi: pilihan saksi | Dua pertanyaan dengan penjelasan. |
| 3 | Tawa tidak berarti setuju | Dengarkan pengakuan Bima tentang tekanan kelompok. |
| 3 | Alasan tidak menghapus dampak | Akui tindakan dan dampaknya kepada Bu Nadia. |
| 3 | Perbaikan dengan batas | Tanyakan batas Dimas → susun langkah tanggung jawab bersama Bu Nadia. |
| 3 | Refleksi: bertanggung jawab | Dua pertanyaan dengan penjelasan. |
| 4 | Tidak perlu menyalahkan diri | Bicarakan tekanan dan perasaan dengan Dimas. |
| 4 | Bukti bukan tontonan | Minta Nara membantu menjaga bukti → minta pendampingan Bu Nadia. |
| 4 | Berlatih menyatakan batas | Sepakati langkah aman bersama Dimas. |
| 4 | Refleksi: hak atas rasa aman | Dua pertanyaan dengan penjelasan. |
| 5 | Saksi yang dapat dipercaya | Pisahkan fakta dari rumor bersama Arga. |
| 5 | Pertemuan dengan persetujuan | Periksa kembali batas Dimas. |
| 5 | Janji harus menjadi tindakan | Periksa tindak lanjut Raka → susun pemeriksaan berikutnya bersama Bu Nadia. |
| 5 | Refleksi: lingkungan yang berubah | Dua pertanyaan dan hasil akhir cerita. |

Semua quest memiliki identitas unik dan prasyarat urut. Identitas quest buku lama dipertahankan. Menemukan buku hanya menyelesaikan `find_notebook`; quest tersebut baru selesai setelah `return_notebook`. Meminta bantuan sebelum mendengar kebutuhan korban tidak melewati urutan objective.

## Bobot percakapan

Setiap percakapan mempunyai fungsi: mengungkap informasi, menunjukkan dampak, meminta persetujuan, menetapkan batas atau menyusun tindakan berikutnya. Menyelesaikan percakapan misi secara normal mengubah objective dan membuka bagian cerita berikutnya. Membatalkan percakapan tidak memberi progress.

Contoh:

- Dimas meminta agar video tangisnya tidak disebarkan. Kalimat itu menjadi batas yang dihormati Nara dalam percakapan berikutnya.
- Raka mengakui bahwa tawa kelompok dipakainya sebagai pembenaran. Bima menjelaskan bahwa tawanya sendiri muncul karena tekanan kelompok.
- Arga mengembalikan buku sambil membedakan bantuan dari kepatuhan atas ancaman. Misi buku selesai tanpa menyelesaikan konflik perundungan secara ajaib.
- Pada chapter terakhir, Nara memeriksa tindakan konkret Raka. Korban tetap dapat mempertahankan jarak dan meminta pendamping.

Dialog prototipe saat ini beralur linear dengan pengaruh pada urutan objective. Belum ada sistem pilihan bercabang yang menghitung kepercayaan atau membuka banyak ending; jangan menyebut mekanik tersebut sudah tersedia. Poin refleksi berasal dari pertanyaan pendidikan dan tidak menilai empati atau nilai diri korban.

## Sistem dan konfigurasi

`StoryChapterSequence` membaca lima `NarrativeChapterData`, memulihkan save, menentukan POV, menjalankan pembukaan, membuka quest berikutnya, menjalankan refleksi dan menampilkan hasil chapter. `PlayablePOVController` memakai satu Player yang berganti seragam/nama; proxy NPC untuk karakter yang sedang dimainkan dinonaktifkan agar tidak ada dua karakter yang sama.

`StoryOpeningDirector` memakai PlayableDirector dan CutsceneManager. Timeline berisi tiga shot Cinemachine dan signal pembuka dialog. Dialog menghentikan Timeline sementara. Shot percakapan diarahkan kepada pembicara; setelah dialog selesai Timeline lanjut, lalu posisi aktor, kontrol dan cursor dipulihkan. Pembukaan dapat dilewati.

`MissionDialogueNPC` memilih percakapan berdasarkan quest aktif, objective yang belum selesai dan objective prasyarat. `EscortCompanion` memakai NavMeshAgent untuk mengikuti Player, berhenti saat dialog/jeda, dan memeriksa kedatangan Player serta Dimas di area aman.

`NotebookCinematicCameraRig` menghitung framing berdasarkan posisi Player dan tiga anggota kelompok. Kamera menyesuaikan jarak/FOV, lalu kembali ke pose asal setelah encounter. Komposisi diberi ruang untuk panel dialog/QTE.

`QuestItem` memisahkan status item, progress quest dan visibilitas renderer. Kegagalan QTE memindahkan item yang sama melalui `QuestItemRehide`, bukan membuat item quest kedua. Empat anchor scene berada di `_Level/NotebookHidePoints`. Lokasi baru harus cukup jauh, berada di NavMesh dan dapat dicapai. Kegagalan meninggalkan objective pencarian belum selesai. Pengambilan berikutnya mengamankan item tanpa mengulang encounter.

`SceneCheckpoint` menyimpan saat gameplay aman atau hasil chapter. Save berisi chapter, POV, progress objective beserta identitas objective, handled event, state/lokasi item, posisi Player dan hasil refleksi. Penulisan memakai file sementara dan penggantian atomik dengan cadangan `.bak`. Save dari nomor chapter lama tanpa prasyarat lengkap kembali ke bagian cerita pertama yang belum selesai.

Setiap refleksi memiliki identitas yang berasal dari ID quest refleksi dan indeks pertanyaan. Jawaban hanya memberi poin sekali. Ketika pemain menekan Lanjutkan, pertanyaan ditandai selesai dan checkpoint ditulis sebelum pertanyaan berikutnya dibuka. Reload melewati pertanyaan yang sudah selesai; mengulang jawaban pada pertanyaan yang belum ditutup tidak menggandakan poin.

`GameplayModalGate` menjaga kontrol dan cursor untuk Quiz, Result dan Pause. Sistem cutscene/dialog tetap memiliki pemulihan masing-masing. Pause mengembalikan state sebelumnya, sehingga QTE atau Timeline tidak dipaksa kembali ke Playing sebelum waktunya.

Menu utama menyediakan Mulai baru, Lanjutkan checkpoint, volume dan Keluar. Mulai baru mengganti progress lama setelah pemain memilih tombol tersebut. Build index: `00_Bootstrap`, `01_MainMenu`, `Level01`.

Karakter memakai delapan model prototipe orisinal berbentuk sederhana, Generic Avatar, Animator blend Idle/Walk, MultiAim untuk kepala dan TwoBoneIK untuk gestur tangan. Model ini adalah aset pengujian mekanik; belum merupakan model humanoid final. Audio prototipe terdiri dari ambience, langkah, objective selesai serta hasil QTE.

Kontrol gamepad dan tombol/stick sentuh menggunakan Input System actions. Wrapper `GameInput.cs` diregenerasi oleh importer Unity; tidak diedit manual. Android menggunakan UI landscape dan safe area; pengujian perangkat serta build Android tetap perlu dilakukan ketika modul/platform tersedia.

## Bukti pengujian yang sudah diperoleh

Pengujian terakhir pada kode dan scene yang tersimpan:

| Pengujian | Hasil |
|---|---|
| `CampaignSmoke.SceneIntegrity` | Lulus 710 cek: lima chapter, prasyarat, objective, referensi, tidak ada missing script dan delapan rig/avatar. |
| `MenuReloadSmoke.Prepare` | Lulus 658 cek pada seluruh scene build: Bootstrap, MainMenu dan Level01 tanpa missing script. |
| `CampaignSmoke.NotebookFailure` | Lulus 56 cek: signal pembukaan, dialog normal/batal, lock kontrol, viewport kamera, QTE/jeda, random rehide, checkpoint lokasi, InteractionDetector nyata, recollect tanpa replay dan turn-in. |
| `SuccessAndControls.Run` | Lulus 37 cek: WASD dan Animator, trigger NPC serta tombol E nyata, teks dialog terlihat, gamepad QTE berhasil, kontrol kembali aktif, turn-in, komposisi kamera/UI dan preview kontrol sentuh. |
| `CampaignSmoke.CompleteCampaign` | Lulus 162 cek: percakapan seluruh misi, NavMesh escort, perpindahan POV/chapter, 22 quest, 10 refleksi, hasil akhir, objective-ID save dan pemulihan backup. |
| `MenuReloadSmoke.Run` | Lulus 44 cek: Bootstrap → menu, tombol settings/continue/new game, restore hasil Chapter 5, pembukaan baru, reload dari Pause, satu instance manager dan LevelLoader. |
| `ReflectionResumeSmoke.Run` | Lulus 16 cek: checkpoint antar pertanyaan, reload saat jawaban belum ditutup, melewati pertanyaan selesai, poin tidak ganda dan pemulihan dari disk. |
| Kompilasi C# | Lulus; tidak ada error kompilasi pada konfigurasi terakhir. |
| Console terakhir | 0 error, tidak sedang compiling; 3 warning paket yang sama dengan laporan build. |
| Startup aplikasi Windows | Lulus cek headless: exe hasil build menjalankan Boot → Loading → MainMenu tanpa error/exception pada log startup. Proses uji ditutup setelah pemeriksaan. |

Screenshot tersimpan di `Logs/AutomationCampaign`, `Logs/AutomationSuccess` dan `Logs/AutomationMenu`. Pemeriksaan visual akhir mengonfirmasi teks dialog terbaca, timer/progress QTE terpasang langsung di panel, judul duplikat sudah hilang, panel refleksi tidak menutupi hasil chapter, dan petunjuk navigasi/nama dunia disembunyikan saat percakapan atau cinematic. Screenshot memakai ScreenCapture agar UI overlay ikut tertangkap.

Tes menggunakan slot save terpisah di `Logs/AutomationCampaign`, `Logs/AutomationSuccess`, `Logs/AutomationMenu` dan `Logs/AutomationReflection`. Data fixture refleksi harus memuat scene melalui state Loading, sama seperti jalur produksi; menjalankan SceneManager.LoadScene langsung dari Result tidak mewakili alur loader. Script pengujian telah disesuaikan dan pengujian ulang lulus.

Build Windows terakhir **Succeeded**, **0 error, 3 warning**, ukuran laporan sekitar 119 MiB. Build berada di `Builds/WindowsPrototype/TheUnseenSide.exe`; laporan lengkap tersimpan di `Logs/WindowsBuildReport.json`. Seluruh folder `WindowsPrototype`, termasuk `_Data`, `MonoBleedingEdge` dan `UnityPlayer.dll`, diperlukan untuk menjalankan aplikasi; jangan menyalin hanya file exe.

Pragma TextMeshPro lama sudah diperbarui mengikuti pesan compiler Unity. Tiga warning tersisa berasal dari paket: Pipeline melaporkan bahwa server runtime sengaja nonaktif, serta SRP membuang `Hidden/Core/DebugOccluder` dan `Hidden/Core/DebugOcclusionTest` karena tag pipeline tidak cocok. Tidak ada shader gameplay yang gagal dikompilasi dalam laporan build. PackageCache tidak diubah untuk menyembunyikan warning. Profiling/perangkat target tetap diperlukan sebelum menyebut build ini siap rilis.

Log startup aplikasi berada di `Logs/WindowsPlayerSmoke.log`. Cek tersebut menggunakan `-batchmode -nographics` agar ringan dan hanya menguji bootstrap/menu aplikasi; verifikasi visual serta alur gameplay lengkap dilakukan di Play Mode Editor. Editor terakhir ditinggalkan dalam Edit Mode pada scene Bootstrap. Opsi Play Mode sementara untuk diagnosis sudah dikembalikan ke pengaturan semula.

## Status roadmap dan batas hasil

| Step | Status |
|---|---|
| 16–21 | Notebook encounter, pendekatan kelompok, dialog, QTE, success/failure, turn-in dan random rehide terpasang; kedua hasil QTE sudah diuji pada konfigurasi terbaru. |
| 22 | Kamera notebook adaptif, pembukaan Timeline, signal, komposisi actor dan UI terpasang serta diverifikasi. |
| 23 | Animator dan Animation Rigging prototipe terpasang; graph runtime diuji valid. Art/animasi final belum diproduksi. |
| 24 | Tiga POV dalam lima chapter sudah dimainkan melalui alur quest nyata. |
| 25 | VideoPlayer, prepare timeout, skip dan fallback tersedia. Belum ada film After Effects final yang diberikan; cutscene utama saat ini real-time 3D. |
| 26–30 | Checkpoint, refleksi, pergantian chapter, audio dasar, menu dan HUD terpasang. Pengujian menu, scene reload, checkpoint refleksi dan UI akhir lulus. |
| 31 | Evaluasi Addressables: data kecil digunakan bersama di satu scene. Referensi langsung dipilih sesuai aturan proyek; paket tersedia tetapi streaming Addressables belum diperlukan. |
| 32–33 | Polish UI/kamera dan pengujian integrasi desktop lulus; build Windows prototipe berhasil. Profiling perangkat target dan optimasi art final belum selesai. |
| 34 | Kontrol sentuh, action gamepad, safe area dan landscape disiapkan. Modul Android belum terpasang pada Editor ini; perangkat fisik belum diuji. |

Untuk belajar, mulai dari quest buku dan lihat hubungan data `QuestData` → `QuestItem` → `BullyingEventDirector` → Timeline signal → Dialogue/QTE → hasil event → objective. Setelah itu lihat `MissionDialogueNPC` dan `StoryChapterSequence` untuk memahami bagaimana percakapan mendorong cerita tanpa menggabungkan seluruh mekanik dalam satu skrip.

## Cara mencoba dan mempelajari hasil

1. Gunakan Editor yang sudah terbuka. Buka scene `Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity` jika belum aktif, lalu tekan Play.
2. Tekan **Mulai baru** untuk alur lima chapter dari awal, atau **Lanjutkan checkpoint** untuk progres yang sudah tersimpan. Mulai baru mengganti checkpoint milik pemain ketika tombol dipilih.
3. Gunakan WASD untuk bergerak, mouse untuk kamera, E untuk berbicara/mengambil, Shift untuk berlari, Space untuk QTE dan Esc untuk jeda. Saat dialog, Space/Enter atau tombol Lanjut menampilkan teks penuh lalu melanjutkan percakapan. Gamepad menggunakan stick, X untuk interaksi, A untuk dialog/QTE, shoulder untuk sprint dan Start untuk jeda.
4. Mulai dengan Dimas. Ikuti petunjuk target di bawah layar dan objective di kiri atas. Permintaan Raka harus didengar sebelum buku bisa diambil; mengembalikan buku adalah objective tersendiri.
5. Untuk membandingkan hasil QTE, jalankan sesi baru: tekan Space berulang agar berhasil, atau biarkan timer habis agar buku dipindahkan ke salah satu titik yang dapat dicapai. Ambil buku yang dipindahkan, lalu kembali ke Raka.

Untuk mengubah cerita, pilih `Assets/_Game/Data/StoryPrototype/Chapter01.asset` sampai `Chapter05.asset`. Inspector menyediakan judul, nama pemain, POV, urutan misi, dialog pembuka, pertanyaan refleksi dan pesan penutup. Dialog tiap misi berada di folder data yang sama dan dihubungkan melalui komponen Mission Dialogue NPC. Mengubah teks aman; pertahankan quest/objective ID dan urutan prasyarat agar save tetap cocok.

Signal dapat dibayangkan sebagai penanda "sekarang jalankan aksi ini" di Timeline. Timeline mengatur waktu dan shot; signal meminta sistem dialog/QTE menjalankan mekanik; manager menentukan hasil; quest mencatat kemajuan. Pemisahan tersebut membuat kegagalan QTE dapat mengubah posisi buku tanpa mengakhiri seluruh quest.

## Pengembangan setelah prototipe

Alur mekanik utama lima chapter sudah tersedia. Pekerjaan berikutnya berfokus pada model humanoid dan animasi final, desain lingkungan outdoor yang lebih rinci, voice-over/musik final, footage After Effects bila diperlukan, serta profiling dan pengujian Android pada perangkat. Pilihan dialog bercabang dapat dikembangkan sebagai fitur berikutnya dengan identitas pilihan dan konsekuensi tersimpan; saat ini percakapan mendorong objective secara linear.
