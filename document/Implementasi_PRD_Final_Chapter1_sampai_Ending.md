# Implementasi PRD Final Chapter 1 sampai Ending

Tanggal pengerjaan 18 November 2026. Baseline memakai PRD FINAL v1 dari Downloads pengguna.

## Hasil integrasi

Chapter 1 mempertahankan tujuh langkah milik LevelOneDirector dan checkpoint v4. ChapterOneCampaignBridge menambahkan hasil chapter dan perpindahan ke Level02 setelah simpan berhasil.

Level02 sampai Level05 memiliki CampaignChapterDirector sebagai pemilik alur masing-masing. QuestManager, LevelOneView, data WordCardData, ActiveInputDeviceTracker, dan InputPromptResolver digunakan kembali. Scene memiliki Aji, kamera gameplay, AreaSekolah, karakter, titik interaksi, dan satu EventSystem aktif.

Authoring lama Chapter 2–5 disimpan sebagai root Legacy authoring (inactive), dengan salinan sebelum perubahan di Logs/BeforeFinalCampaign. Placeholder prefab SchoolEnvironment yang referensinya hilang dihapus pada Chapter 3–5.

## Data dan source

- Assets/_Game/Data/CampaignPRD/CampaignContent.json memuat naskah dan konfigurasi tugas hasil pemetaan PRD.
- Assets/_Game/Data/CampaignPRD/Chapter2.asset sampai Chapter5.asset memuat urutan langkah.
- Lima data WordCardData baru digunakan untuk dukungan Radit, Sikka, pelaporan, kesepakatan kelas, dan refleksi akhir.
- Assets/_Game/Scripts/Campaign memuat controller chapter, checkpoint, challenge, cinematic, evade, world props, dan bridge Chapter 1.
- Assets/_Game/Scripts/Editor/FinalCampaignAuthoring.cs menyediakan authoring scene.
- Assets/_Game/Scripts/Editor/FinalCampaignSmoke.cs menyediakan tes interaksi lintas chapter.
- LevelOneView ditambah fungsi HUD kampanye, pilihan tugas, dan skip cinematic.
- MainMenuFlow mendeteksi checkpoint kampanye final untuk Continue serta meresetnya saat New Game.

## Chapter 2

Empat belas langkah mencakup izin bantuan Radit, wawancara Bu Lilis/Pak Jaya, tiga clue dunia, Clue Map, penyusunan rak, pickup dan return kacamata, Kartu Kata, cinematic ancaman, evade ke area Bu Lilis, dan laporan.

Clue Map serta rak memakai pilihan berurutan. Kesalahan mempertahankan jawaban benar. Kacamata berupa prop sederhana yang berpindah ke grip Aji dan anchor kepala Radit berdasarkan checkpoint.

Evade memakai gerak third-person existing, tombol Left Ctrl atau tombol kanan gamepad untuk menghindar. Ada tiga telegraph, dua peringatan menyebabkan retry, dan mode mudah memperpanjang antisipasi. Keberhasilan dicapai dengan memasuki area aman. Input attack tidak ditambahkan.

## Chapter 3

Sebelas langkah mencakup izin bicara, peninjauan pesan yang disetujui Sikka, enam klasifikasi komentar, pengumpulan tiga bahan, susun studio, pilihan PRIVATE/LIMITED/SHOW, Kartu Kata, pilihan cara laporan, dan dukungan wali kelas.

Pilihan privasi tersimpan dalam save kampanye. PRIVATE/LIMITED mendapat dialog yang menghormati keputusan Sikka. Prop karya pada ending hanya diperlihatkan untuk pilihan SHOW.

## Chapter 4

Delapan langkah mencakup jurnal bersama Fatimah, pengakuan Raka, kronologi enam peristiwa, enam pertanyaan Evidence Board, Kartu Kata laporan, intake BK, cinematic Memet/Pak Damar, dan rencana perlindungan.

Evidence Board memeriksa tiga kasus serta sumbernya, penanganan privat Sikka, dan penolakan tiga klaim tanpa bukti. Pertanyaan benar tersimpan ketika pertanyaan berikutnya salah. Dialog konseling Memet ditampilkan sebagai cinematic pemain. Rincian keluarga tidak disalin ke jurnal publik Aji.

## Chapter Ending

Sepuluh langkah mencakup cinematic beberapa minggu kemudian, pengambilan bahan, tiga stasiun proyek, kesepakatan kelas, follow-up BK, pilihan karya Sikka, refleksi bersama, dan cinematic gerbang.

Setelah tamat tersedia Credits, jelajahi epilog, dan menu utama. Pilihan pribadi Sikka tidak mengunci ending.

## Scene cinematic baru

Setiap scene memiliki environment, karakter, kamera render/AudioListener lokal, kamera Cinemachine, serta aset Timeline. Dialog ditampilkan melalui UI existing pada hub selama scene cinematic aktif. Tombol Lewati adegan menyelesaikan request yang sama.

- Level_02/Cutscene/CS_L02_AncamanKoridor.unity
- Level_04/cutscene/CS_L04_IntakeBK.unity
- Level_04/cutscene/CS_L04_BKMemet.unity
- Level_05/cutscene/CS_L05_IntroKelasAman.unity
- Level_05/cutscene/CS_L05_Penutup.unity

Pipeline menyembunyikan renderer hub, menjeda kamera/audio/lighting hub, memuat scene additive, memainkan Timeline dan dialog, lalu memulihkan hub. BekalCutscene tetap dikelola LevelOneDayTwoFlow.

## Save dan kompatibilitas

File unseen-campaign-final-v1.json terpisah dari unseen-level01-day-two.json. Save mencatat active/unlocked chapter, step ID, posisi Aji, jawaban tugas, bank urutan, slot kata, keputusan privasi, jurnal, dan status tamat.

Penulisan memakai temporary file, validasi, dan backup .bak. Editor mendukung SessionState CampaignPRD.TestSave untuk fixture pengujian. Checkpoint C1 tetap memakai skema v4.

## Keputusan produksi

Model dan animasi karakter diambil dari Assets/_Game/Art/Characters. Karakter yang sudah ada di Level01 dipakai sebagai sumber untuk Aji, Bima, Radit, Memet, Raka, Bu Ratna, Vivi, Nando, Sahrul, dan Sikka.

Laura menggunakan model berkulit dari aset animasi Laura. Bu Lilis memakai model Laura sementara. Pak Damar dan Pak Jaya memakai model staf Pak Arif dan Pak Beni. Penampilan dua staf ini perlu persetujuan artistik pengguna.

Buku, bingkai kacamata, poster, serta bagian karya/studio memakai prop bentuk sederhana. Pose/shot cinematic dan rutinitas NPC masih dasar. Estimasi durasi 2,5–3,5 jam pada PRD memerlukan playtest pacing dan produksi cinematic lebih lanjut.

Rangkaian Prolog berada di luar perubahan kelanjutan Chapter 1–ending ini. Integrasi Tutorial/Prolog yang sudah ada pada menu dipertahankan.

## Pengujian

Lihat Logs/FinalCampaignSmoke.txt untuk hasil aktual. Tes berjalan melalui interaksi controller dan callback tombol UI, memakai checkpoint terisolasi. Tes memindahkan Aji ke titik objective dan area aman evade agar memeriksa gating, puzzle, dan transisi tanpa bergantung pada input manual.

Kelulusan tes ini membuktikan alur fungsional yang diuji. Pemeriksaan rute berjalan manual, dodge dengan stik fisik, quality review semua shot, profiler FPS, serta build Windows tetap diperlukan.

## Hasil QA akhir sesi

- Tes interaksi melintasi 43 langkah Chapter 2–5 dan mencapai ending → menu utama. Log berada di Logs/FinalCampaignSmoke.txt.
- Percobaan pertama timeout saat intro Chapter 5. Pengujian dilanjutkan dari checkpoint stabil; seluruh langkah Chapter 5 berhasil. Batas waktu smoke diperpanjang menjadi 180 detik untuk penundaan Editor.
- Checkpoint C1 v4 yang sudah selesai berhasil dipulihkan dan tombol Lanjut ke Chapter 2 memuat Level02.
- Checkpoint tamat berhasil dipulihkan; Credits dapat dibaca sampai selesai, jelajah epilog terbuka, dan tombol interaksi membuka sapaan Bima.
- Jawaban Evidence Board yang salah mempertahankan langkah dan menambah feedback. Satu jawaban benar tersimpan serta dipulihkan setelah reload scene.
- Evade diuji dengan Aji diam sampai dua ancaman. Retry lokal berhasil, mode mudah dapat dimulai, dan mencapai area aman membuka laporan.
- Penempatan cadangan Memet ditambahkan ketika titik NavMesh untuk telegraph tidak ditemukan.
- Audit scene tersimpan memastikan masing-masing Level02–05 memiliki satu owner kampanye aktif, satu kamera render aktif, satu AudioListener aktif, dan satu EventSystem aktif.
- Jumlah langkah tersimpan per scene adalah 14, 11, 8, dan 10. Semua step memiliki QuestData dan ID unik.
- Fixture pengujian memakai file Logs. Override Editor dikosongkan setelah tes.

Epilog memiliki tambahan model Bima, Radit, Nando, Sahrul, Memet, dan Raka. Sapaan opsional mengikuti karakter terdekat. Verifikasi Play Mode menggunakan pemindahan posisi ke objective dan callback tombol untuk tes alur. Pemain fisik, seluruh rute berjalan, penataan visual final, VO, serta build Windows masih memerlukan pengujian produksi.

## Pembaruan gameplay dan Hierarchy

Di Tutorial, panel GPS dan petunjuk bergerak mengikuti perangkat input terakhir. Tombol M atau Select membuka peta. Fokus awal mengarah ke tujuan yang tersedia sehingga pemain bisa memilihnya dengan stik.

Di Level01–05, panel percakapan berada di bawah layar. Nama pembicara memakai Super Bouncer dan isi dialog memakai Cleanow. Pada cinematic, tombol Lanjut dan Lewati adegan berada di panel yang sama.

Pada Level02–05, pilih objek Campaign Level di Hierarchy. Di bawah Quest Points, tiap grup seperti L02-Q02 berisi titik interaksi untuk satu misi. Properti misi mengelompokkan benda dunia dan posisi akhirnya. Penanda tujuan menunjukkan tempat interaksi di lapangan. Setiap scene memiliki NavMesh dari collider AreaSekolah. Panah HUD mengikuti belokan yang bisa dilalui dan menampilkan jarak lurus ke tujuan.

Pencarian kacamata kini membedakan kedua saksi, buku di lorong, catatan troli, meja temuan, serta pengambilan kacamata. Di studio Sikka, setiap pengambilan memindahkan bahan ke meja kerja. Di proyek kelas, pemain memilih tempat poster dan cara merapikan jalur pintu. Jurnal menaruh tujuan saat ini di atas catatan terbaru. Pilihan Sikka untuk berbagi karya dan menyampaikan laporan menghasilkan tanggapan yang sesuai.

Periksa Logs/FinalCampaignPolishQA.txt untuk hasil pemeriksaan rute dan Logs/FinalCampaignSmoke.txt untuk hasil tes interaksi. Pemeriksaan NavMesh menghitung jalur dari spawn ke tujuan pertama dan dari setiap tujuan ke tujuan berikutnya.
