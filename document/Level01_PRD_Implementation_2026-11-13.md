# Level01 — PRD Kartu Aksi × Kartu Kata

**Revisi integrasi hierarki asli:** alur live sekarang memakai GuideController, AmbushTriggerZone, YellowCircleTrigger dan BekalCutsceneTrigger yang sudah ada, beserta Timeline MasukKelas dan Canteen. Baca `Level01_Edit_Zona_dan_Cutscene.md` sebagai panduan aktif. Kamera dan salinan pemeran prototipe awal dinonaktifkan. Angka 143 assertion di bawah adalah hasil prototipe sebelum revisi ini; laporan alur asli terpisah di `Logs/LevelOnePRD/ExistingFlow-Smoke.txt`.

Acuan: `UNSEEN_SIDE_PRD_Level_1_dan_2.md` (13 November 2026). Implementasi ini khusus Level 1 di scene `Assets/_Game/Scenes/Levels/Level_01/Level01.unity`. Level 2–5 tidak dimigrasikan. Scene pengguna yang sudah berisi AreaSekolah, karakter, dan Timeline menjadi dasar; bukan hasil membangun ulang lingkungan PRD lama.

## Menjalankan dan mengatur posisi

1. Buka **Level01**, kemudian Play. WASD bergerak, mouse mengatur kamera, E menjalankan Kartu Aksi di dekat penanda; 1–3 memilih kartu kontekstual; TAB membuka jurnal; ESC membuka jeda. Puzzle memakai klik frasa berurutan lalu **Ucapkan**, tanpa timer.
2. Di Hierarchy buka **[Level01 PRD - Langkah Pertama] → 01 - Titik Interaksi (pindahkan dengan Move Tool)**. Setiap titik adalah GameObject tersendiri dengan `LevelOnePoint`, radius interaksi, label dan ID stabil. Pindahkan Transform di Edit Mode; jangan mengganti ID.
3. Titik tujuan, jarak interaksi, arah HUD, dan visual barang membaca Transform yang sama. Letakkan pada permukaan yang bisa dicapai pemain. Memindahkan marker tidak membangun ulang NavMesh atau mengubah bangunan/prop lingkungan asli.
4. **02 - Posisi Aktor dan Rute** memuat waypoint Bima dan posisi panggung Memet/Raka. Posisi aktor mengikuti penanda pada awal/masuk tahapan cerita; Bima dan Fatimah tetap dapat bergerak sebagai pendamping.
5. **03 - Pemeran PRD** memakai salinan rig yang sudah ada. Model Fatimah dan petugas masih sementara. **04 - Kamera Adegan PRD** adalah kamera Cinemachine untuk adegan pendek. **05 - Penghubung NavMesh Pintu VIII A** menjembatani celah NavMesh antara lantai koridor dan kelas; kedua ujungnya dapat dipindahkan.
6. Gunakan **ESC → Ulang Level 1** untuk mengulang dengan konfirmasi. Checkpoint normal otomatis melanjutkan progres saat Play berikutnya.

| Penanda | Peran |
| --- | --- |
| MeetBima | TANYA Bima di gerbang; mulai pendampingan ke kelas. |
| ClassFront | AMATI papan kelas VIII A; Bima harus sampai bersama Aji. |
| Corridor | Insiden Memet dan Raka; adegan 3D pendek dengan Cinemachine. |
| BimaCorridor | K-01, baru terbuka setelah pelaku pergi. |
| Teacher | Bu Ratna memperkenalkan Aji. |
| Sikka | Kursi kosong; catatan Sikka tanpa spekulasi penyebab absennya. |
| Canteen | Insiden pengambilan barang, satu kali dalam progres quest. |
| BimaCanteen | K-02 menawarkan bantuan dengan izin korban. |
| Fatimah | Saksi, petunjuk dan pendamping pencarian. |
| Staff | Izin mengambil botol di rak area kerja; petugas menyerahkannya. |
| P1 | Meja kantin: petunjuk menuju rak dan bangku. |
| P2 | Tas bukan milik Bima: tidak pernah masuk inventori. |
| P3 | PERIKSA lalu AMBIL BOTOL-BIMA; butuh izin petugas. |
| P4 | Pot besar: petunjuk ke bangku pojok. |
| P5 | PERIKSA lalu AMBIL BEKAL-BIMA, di bawah bangku terbuka. |
| Bench | KEMBALIKAN 2/2, K-03 dukungan, lalu CATAT dan refleksi penutup. |

## Data dan integrasi

`Assets/_Game/Data/LevelOnePRD/` berisi lima QuestData L1-Q01–Q05, tiga WordCardData K-01–K-03, dan Level01_LangkahPertama berisi 13 langkah. Quest tetap memakai QuestManager proyek. Intent puzzle divalidasi dengan ID frasa dan urutannya, bukan pencocokan kalimat. Dua kesalahan membuka hint; empat membuka bantuan susun. Salah menjawab tidak memberi progres atau penalti.

Barang memiliki flag ditemukan, diambil, dan dikembalikan; visual mengikuti state. Pencarian tidak mengharuskan semua distraktor diperiksa. HUD menampilkan 0/2, 1/2, 2/2; botol dan bekal dapat diambil dalam kedua urutan. Hint pencarian tambahan muncul setelah 90 detik tanpa progres. Jurnal menyimpan petunjuk, informasi Sikka, kronologi, izin dan dukungan, serta tiga gagasan refleksi akhir.

Checkpoint baru: `Application.persistentDataPath/unseen-level01-cards-v1.json`, dengan file sementara tervalidasi dan backup `.bak`. Save kampanye PRD 1.1 sebelumnya tetap terpisah. Menu utama lama belum dimigrasikan ke skema baru; untuk demo ini jalankan Level01 langsung dan gunakan menu jeda Level 1 untuk mengulang.

Skrip berada dalam Core/LevelOne, Dialogue, Interaction, NPC, Events, Inventory, Save dan UI. `LevelOnePRDAuthoring` adalah authoring khusus scene ini. Menu Apply menolak membangun ulang bila root sudah ada, supaya posisi yang telah disunting tidak tertimpa. Jangan menjalankan authoring PRD 1.1 lama karena itu memigrasikan lima scene ke rancangan terdahulu.

Pemicu/misi/UI lama pada Level01 dinonaktifkan agar tidak bersaing dengan alur ini. Objek lama dan Timeline tetap berada dalam scene. Daftar komponen yang dinonaktifkan tercatat di `Logs/LevelOnePRD/DisabledLegacyBehaviours.txt`. Backup scene sebelum migrasi: `Logs/BeforeLevelOnePRD/Level01-20261113-133240.unity` (salinan lokal, bukan scene Build Settings).

## Verifikasi dan batas produksi

Runner `LevelOnePRDSmoke.Begin(0)` dan `Begin(1)` berjalan di Editor yang sama dengan save uji terpisah di Logs. Keduanya menguji quest penuh, input di luar jangkauan, penguncian K-01, izin petugas, tas bukan sasaran, urutan frasa salah, hint/bantuan, dua urutan pengambilan barang, serta keluar/masuk Play Mode pada progres 1/2. Laporan: `Logs/LevelOnePRD/Smoke-0.txt` dan `Smoke-1.txt`. Override save uji dihapus setelah selesai. Runner memindahkan pemain antartitik; bukan bukti playtest berjalan manual seluruh rute atau durasi 15–25 menit.

Hasil: **73 + 70 assertion integrasi lulus**. Audit 10 sambungan navigasi menghasilkan `PathComplete` setelah penambahan link pintu kelas. Uji CharacterController bergerak melalui ambang pintu lulus sesudah `Step Offset` pemain diperbaiki dari 0,01 menjadi 0,35 m. Laporan tambahan: `Navigation.txt` dan `DoorwayTraversal.txt` di folder log yang sama. Layout Kartu Kata diperiksa pada screenshot 1920 × 1080; ini bukan uji seluruh ukuran layar atau penilaian usability.

Presentasi masih prototipe: gerak dan panggung 3D pendek, model karakter yang dipakai ulang, dan beberapa prop sederhana. Belum ada animasi dorongan/pengambilan barang final, lip-sync, voice-over, atau validasi kenyamanan oleh siswa/guru BK. Rancangan Level 2 dalam PRD tersalin sebagai referensi dan belum dieksekusi.
