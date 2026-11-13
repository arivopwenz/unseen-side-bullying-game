# UNSEEN SIDE

**PRODUCT REQUIREMENTS DOCUMENT**

Level 1 — Langkah Pertama • Level 2 — Luka yang Tak Terlihat

MEKANISME INTI • KARTU AKSI × KARTU KATA

> **DEFINISI PRODUK**
> Kartu Aksi adalah perbuatan yang pemain lakukan di dunia 3D (memeriksa, mengambil, mengembalikan, mendampingi, menghindar). Kartu Kata adalah puzzle menyusun potongan kalimat yang tepat untuk berinteraksi secara empatik dan aman. Tujuan quest tetap ditampilkan dalam jurnal, bukan sebagai mekanik kartu ketiga.


| **Metadata**      | **Spesifikasi**                                        |
|-------------------|--------------------------------------------------------|
| Versi             | 1.0 — usulan desain untuk prototipe TA                 |
| Tanggal           | 13 November 2026                                       |
| Platform / engine | PC Windows / Unity 3D (usulan)                         |
| Format            | Single-player, 3D third-person, story-driven adventure |
| Cakupan           | Prolog singkat sebagai prasyarat + Level 1 dan Level 2 |

# 01. Ringkasan eksekutif

PRD ini menetapkan kebutuhan produk untuk dua level pertama UNSEEN SIDE, game petualangan edukasi anti-bullying berlatar SMP Indonesia. Pemain berperan sebagai Aji, siswa baru yang pada awalnya belum mengetahui mengapa Bima takut pada Memet. Melalui peristiwa nyata dalam cerita, pemain menggunakan Kartu Aksi untuk bertindak dan Kartu Kata untuk menyusun respons yang tepat.

> **PERUBAHAN DESAIN DARI GDD V2**
> GDD sebelumnya menggunakan istilah Kartu Quest dan Kartu Aksi. Revisi ini menetapkan Kartu Aksi dan Kartu Kata sebagai dua mekanik aktif utama. “Quest” menjadi sistem tujuan/checklist di HUD dan jurnal agar player tetap tahu target, tanpa perlu memainkan kartu quest sebagai item.


## 1.1 Masalah yang ingin diselesaikan

- Game edukasi berisiko menjadi cutscene pasif; pemain perlu melakukan tindakan konkret yang merepresentasikan perilaku menolong secara aman.
- Puzzle eksplorasi tanpa makna narasi terasa seperti mencari benda acak; setiap petunjuk perlu berasal dari saksi, korban, atau lingkungan.
- Dialog bercabang biasa berisiko menjadi tebak jawaban; puzzle Kartu Kata membuat pemain secara aktif menyusun kalimat yang tidak menyalahkan korban atau memprovokasi pelaku.
- Konfrontasi fisik harus dramatis tanpa mengajarkan balasan kekerasan; sistem menghindar mengarahkan pemain ke jarak aman dan bantuan orang dewasa.

## 1.2 Sasaran dan keluaran pembelajaran

| **Sasaran pemain**  | **Dampak yang diinginkan**                                                                    |
|---------------------|-----------------------------------------------------------------------------------------------|
| Gameplay            | Mengerti kapan memakai Kartu Aksi dan Kartu Kata tanpa membaca instruksi panjang.             |
| Literasi bullying   | Mengenali ejekan berulang, pengambilan barang, pemaksaan, ancaman, dan dampaknya.             |
| Sikap sebagai saksi | Memilih bantuan aman, mendengarkan korban, meminta izin, dan melibatkan guru saat diperlukan. |
| Komunikasi          | Menyusun ucapan yang mendukung korban atau menetapkan batas pada pelaku tanpa provokasi.      |

## 1.3 Batasan versi Level 1–2

Termasuk: perkenalan Aji dan Bima, adegan koridor, insiden barang Bima di kantin, Radit dan kacamata di perpustakaan, konfrontasi singkat tanpa menyerang balik, tutorial kartu, jurnal quest, checkpoint, serta refleksi pelajaran. Tidak termasuk: cyberbullying penuh, kisah Sikka yang diselesaikan, misteri keluarga Memet, dan proses formal BK/ending kelas; semuanya disiapkan sebagai hook untuk Level 3–5.

# 02. Pengguna, pengalaman, dan prinsip desain

## 2.1 Profil pemain dan konteks

| **Aspek**    | **Persyaratan**                                                                                     |
|--------------|-----------------------------------------------------------------------------------------------------|
| Target utama | Siswa SMP kelas VII–IX / ±12–15 tahun; uji dengan kelompok yang sesuai izin sekolah.                |
| Keterampilan | Pemain kasual, belum tentu terbiasa menggunakan game third-person.                                  |
| Bahasa       | Dialog bahasa Indonesia sehari-hari yang natural dan sopan; teks UI sederhana.                      |
| Sesi bermain | Level 1 sekitar 15–25 menit; Level 2 sekitar 20–30 menit (target awal, perlu playtest).             |
| Gaya visual  | 3D stylized semi-realistis; SMP Indonesia, genteng merah, warna navy, biru, krem, dan aksen kuning. |

## 2.2 Prinsip produk

- Belajar lewat tindakan: edukasi muncul sebagai akibat keputusan, bukan ceramah panjang.
- Korban didengar: dialog untuk korban berisi validasi dan pilihan bantuan, bukan nasihat agar korban “sadar” atau disalahkan.
- Pelaku tidak langsung berubah karena satu kalimat; puzzle hanya membantu mengakhiri situasi dengan aman.
- Kartu diperoleh melalui cerita dan konteks, bukan acak, gacha, atau membeli.
- Kesalahan menjadi kesempatan belajar: feedback + retry yang aman tanpa mempermalukan pemain.

## 2.3 Core gameplay loop

| **Tahap**          | **Aksi player**                                  | **Output sistem**                    |
|--------------------|--------------------------------------------------|--------------------------------------|
| 1. Peristiwa      | Menemui NPC / melihat kejadian                   | Quest aktif + konteks tersedia       |
| 2. Pahami masalah | Dengar petunjuk dan lihat lingkungan             | Titik interaksi / clue terungkap     |
| 3. Kartu Aksi     | Inspeksi, ambil, dampingi, hindar, minta bantuan | State objek/karakter berubah         |
| 4. Kartu Kata     | Pilih dan urutkan keping kata/kalimat            | Respons diucapkan + feedback edukasi |
| 5. Validasi       | Cek barang, keselamatan, dan respons             | Quest complete / soft retry          |
| 6. Refleksi       | Baca pesan akhir bab                             | Pelajaran tercatat di jurnal         |

# 03. Spesifikasi dua kartu inti

## 3.1 Kartu Aksi — interaksi nyata dalam game

Kartu Aksi adalah ability kontekstual yang memicu gerakan Aji, inspeksi objek, interaksi NPC, atau mode mini-game. Pemain tidak bebas menggunakan semua aksi di semua tempat; kartu hanya aktif ketika target dan syarat aman terpenuhi.

| **Field kartu**  | **Perilaku sistem**                                                                    |
|------------------|----------------------------------------------------------------------------------------|
| ID + nama        | Misalnya A-03 PERIKSA; nama pendek, ikon jelas.                                        |
| Konteks          | Area, jarak, objek/NPC, status quest, dan izin tindakan.                               |
| Input            | Tombol 1–3 pilih kartu aktif; E eksekusi; klik jika menu kartu terbuka.                |
| Hasil            | Animasi + perubahan state, log event dan progres checklist.                            |
| Status           | Locked → Available → Active → Resolved; aksi tertentu reusable.                        |
| Batas penggunaan | Tidak bisa mengambil barang milik NPC sembarangan; “HINDAR” hanya aktif ketika bahaya. |

## 3.2 Katalog Kartu Aksi L1–L2

| **ID** | **Nama**      | **Fungsi & tempat**                                                  |
|--------|---------------|----------------------------------------------------------------------|
| A-01   | AMATI         | Memeriksa ekspresi, petunjuk, lokasi atau situasi di koridor/kantin. |
| A-02   | TANYA         | Mengajak saksi/korban memberi petunjuk secara sukarela.              |
| A-03   | PERIKSA       | Membuka mini-inspeksi titik pencarian, dengan highlight prop.        |
| A-04   | AMBIL         | Mengambil barang sasaran yang telah ditemukan dan aman dijangkau.    |
| A-05   | KEMBALIKAN    | Menyerahkan barang kepada pemilik, memeriksa kelengkapannya.         |
| A-06   | DAMPINGI      | Berjalan bersama korban ke tempat aman / membantu duduk.             |
| A-07   | MINTA BANTUAN | Memanggil pustakawan, wali kelas, atau petugas keamanan.             |
| A-08   | HINDAR        | Gerak dodge untuk menjauh dari serangan; tidak memberi damage.       |
| A-09   | CATAT         | Merekam kronologi berupa ringkasan, tanpa memotret ulang korban.     |

## 3.3 Kartu Kata — puzzle menyusun kalimat

Kartu Kata bukan pilihan jawaban tunggal. Pemain menerima beberapa keping frasa, lalu mengurutkan 2–3 bagian inti menjadi satu kalimat yang sesuai dengan konteks. Ada satu sampai dua keping pengecoh. Kartu Kata dapat digunakan untuk berbicara dengan korban (validasi/menawarkan bantuan), saksi (meminta informasi), atau pelaku (menetapkan batas dan mencari keselamatan).

| **Tahap**            | **Detail interaksi**                                                                                              |
|----------------------|-------------------------------------------------------------------------------------------------------------------|
| Pemicu               | Dialog sosial / kondisi aman setelah aksi / NPC meminta bantuan.                                                  |
| Tampilan             | Teks konteks NPC, 3 slot kalimat, 5–6 keping frasa, tombol “Ucapkan”.                                             |
| Operasi              | Drag-and-drop keping atau klik keping berurutan (aksesibilitas).                                                  |
| Validasi             | Berdasarkan ID intent dan urutan slot, bukan kecocokan string yang rapuh.                                         |
| Feedback benar       | Aji mengucap kalimat; NPC bereaksi; jurnal mencatat alasan kalimat aman.                                          |
| Feedback belum tepat | “Kalimat ini terdengar menyalahkan/memprovokasi. Coba fokus pada dukungan atau keselamatan.” Retry tanpa penalti. |
| Bantuan              | Setelah 2 kegagalan, tampil hint tentang maksud kalimat; setelah 4, opsi susun otomatis.                          |

## 3.4 Tingkat kesulitan Kartu Kata

| **Tahap**            | **Bentuk puzzle**          | **Penyajian**                              |
|----------------------|----------------------------|--------------------------------------------|
| Level 1 awal         | 2–3 frasa inti, 1 pengecoh | Tutorial, tanpa timer; frasa pendek.       |
| Level 1 akhir        | 3 frasa, 2 pengecoh        | Fokus tawarkan bantuan dan tidak memaksa.  |
| Level 2 perpustakaan | 3 frasa, 2 pengecoh        | Gabungkan bantuan konkret + orang dewasa.  |
| Level 2 konfrontasi  | 3 frasa, 2 pengecoh        | Hanya terbuka saat Aji mencapai zona aman. |

> **ATURAN EDUKASI UTAMA**
> Kartu Kata bukan alat “menyadarkan korban”. Kepada korban, puzzle membangun empati dan menawarkan bantuan. Kepada Memet, puzzle mengajarkan batas yang tegas dan tidak memprovokasi; tidak membuat Memet langsung berubah atau memaafkan diri sendiri.


# 04. UI, UX, dan kontrol

## 4.1 Hierarki UI saat bermain

| **Elemen**       | **Posisi / isi**                           | **Aturan**                                            |
|------------------|--------------------------------------------|-------------------------------------------------------|
| Quest tracker    | Kiri atas: judul + 1 langkah aktif         | Tidak memakan layar; TAB membuka semua tahapan.       |
| Kartu Aksi       | Bawah kanan: maksimal 3 kartu situasional  | Hanya kartu relevan; ikon + tombol cepat 1–3.         |
| Prompt interaksi | Dekat objek/NPC: “E • PERIKSA RAK”         | Muncul hanya dalam jarak interaksi dan line of sight. |
| Kartu Kata       | Panel tengah saat mode dialog puzzle       | Gameworld dipause / NPC menunggu agar bebas membaca.  |
| Jurnal           | TAB: quest, petunjuk, barang, pembelajaran | Progres tersimpan; tidak mengungkap spoiler.          |
| Umpan balik      | Toast singkat + suara ringan               | Tidak memberi angka “poin mengalahkan pembully”.      |

## 4.2 Input awal PC

| **Kontrol** | **Fungsi**                                                |
|-------------|-----------------------------------------------------------|
| WASD        | Bergerak                                                  |
| Mouse       | Arah kamera third-person                                  |
| E           | Interaksi / inspeksi objek di dekat Aji                   |
| 1 / 2 / 3   | Memilih Kartu Aksi yang tersedia                          |
| Klik / drag | Menyusun keping Kartu Kata                                |
| Space       | Menghindar saat encounter / tidak dipakai untuk menyerang |
| Tab         | Jurnal quest, kartu, petunjuk                             |
| Esc         | Pause, pengaturan, bantuan kontrol                        |

## 4.3 Aksesibilitas dan keamanan

- Teks dialog bisa dipercepat/diulang; subtitle minimal setara 18–20 px pada 1080p, skala dapat diubah.
- Puzzle dapat diselesaikan dengan klik saja selain drag; tidak ada keharusan mengetik cepat.
- Dodge memakai telegraph jelas; tersedia Assist Mode berupa slow-motion/auto-dodge tanpa memotong pelajaran.
- Tidak ada health bar atau efek reward ketika seseorang terluka; jika gagal, pindah ke checkpoint aman.
- Tidak menampilkan konten bullying sebagai lelucon atau koleksi yang bisa disebarkan kembali.

# 05. Level 1 — Langkah Pertama

Tujuan naratif: Aji pertama kali masuk sekolah, bertemu Bima, dan mulai curiga bahwa tindakan Memet bukan candaan biasa. Ia belum memahami pola perundungan sampai mengalami quest pengambilan barang Bima. Fokus edukasi: perhatikan dampak candaan, hormati barang pribadi, dengarkan korban.

| **Parameter**      | **Target desain**                                                 |
|--------------------|-------------------------------------------------------------------|
| Lokasi             | Gerbang → koridor → VIII A → kantin → bangku taman.               |
| Durasi target      | 15–25 menit termasuk tutorial kartu.                              |
| NPC kunci          | Aji, Bima, Memet, Raka, Fatimah, Bu Ratna.                        |
| Kartu Aksi dominan | AMATI, TANYA, PERIKSA, AMBIL, KEMBALIKAN, CATAT.                  |
| Kartu Kata dominan | K-01 cek keadaan Bima, K-02 tawarkan bantuan, K-03 dukung korban. |
| Exit chapter       | Barang 2/2 kembali, Bima didengar, catatan insiden tersimpan.     |

## 5.1 Alur quest Level 1

| **ID / scene**   | **Objective terlihat player**                    | **Kondisi berhasil**                                             |
|------------------|--------------------------------------------------|------------------------------------------------------------------|
| L1-Q01 / Gerbang | Temukan Bima dan kelas VIII A.                   | Interaksi Bima; sampai di depan kelas.                           |
| L1-Q02 / Koridor | Lihat situasi saat Memet menghadang.             | Aji merespons tanpa menuduh/bertengkar; K-01 selesai.            |
| L1-Q03 / Kelas   | Kenali lingkungan dan kursi kosong Sikka.        | Perkenalan kelas; bendera info Sikka tersimpan.                  |
| L1-Q04 / Kantin  | Cari bekal dan botol Bima dari petunjuk Fatimah. | Saksi diajak; dua item ditemukan; tidak memasuki area terlarang. |
| L1-Q05 / Bangku  | Kembalikan barang dan dengarkan Bima.            | Dua item diserahkan; K-03 selesai; jurnal tercatat.              |

## L1-S01 Gerbang: teman pertama

Lokasi: Gerbang, koridor \| Tujuan adegan: Tutorial interaksi

Aji mencari kelas VIII A dan bertemu Bima di dekat taman. Bima bersedia mengantar. Kamera tetap third-person saat berjalan bersama.

> Aji: “Hai, aku Aji. Kamu tahu kelas VIII A di mana?”
> Bima: “Kebetulan aku kelas VIII A. Yuk, aku anter!”


**Kartu Aksi:** A-01 AMATI papan kelas; A-02 TANYA Bima.

**Kartu Kata:** Belum ada; tutorial interaksi.

**Selesai jika:** Mencapai marker kelas bersama Bima.

**Pelajaran:** Meminta petunjuk adalah bagian dari beradaptasi di tempat baru.

## L1-S02 Dihadang Memet dan Raka

Lokasi: Koridor kelas \| Tujuan adegan: Perkenalan konflik secara bertahap

Memet mengejek dan mendorong bahu Bima. Aji yang baru pindah tidak tahu sebabnya. Pemain tidak diminta menghakimi Memet saat itu juga.

> Memet: “Wih, Bima! Sekarang punya temen baru, nih!”
> Raka: “Tumben ada yang mau jalan bareng!”
> Aji: “Eh, Bim, kamu nggak apa-apa?”
> Bima: “Iya... udah biasa kok.”


**Kartu Aksi:** A-01 AMATI ekspresi Bima.

**Kartu Kata:** K-01 “Tanyakan kondisi Bima” (tutorial puzzle).

**Selesai jika:** Puzzle K-01 selesai; Bima dan Aji melanjutkan ke kelas.

**Pelajaran:** Jika seseorang tampak takut, “cuma bercanda” belum tentu aman.

## L1-S03 Bangku kosong di kelas

Lokasi: VIII A \| Tujuan adegan: Memperkenalkan dampak lain tanpa spoiler

Bu Ratna memperkenalkan Aji. Ia memperhatikan bangku Sikka yang kosong, tetapi informasi tentang Sikka belum menjadi quest utama.

> Bu Ratna: “Teman-teman, ini Aji. Tolong bantu dia beradaptasi.”
> Aji: “Bim, bangku itu biasanya ditempati siapa?”
> Bima: “Sikka. Belakangan dia sering nggak masuk.”


**Kartu Aksi:** A-01 AMATI papan ekskul / kursi.

**Kartu Kata:** Tidak ada puzzle wajib.

**Selesai jika:** Bicara dengan Bima dan melihat petunjuk kursi kosong.

**Pelajaran:** Jangan menyebarkan dugaan tentang teman yang sedang tidak masuk.

## L1-S04 Bekal dan botol diambil

Lokasi: Kantin \| Tujuan adegan: Pencarian multi-petunjuk

Memet mengambil bekal dan botol Bima; Fatimah melihat arah ia pergi. Aji tidak mengejar pelaku ke tempat sepi. Player bertanya pada saksi lalu memeriksa lima lokasi terbuka.

> Memet: “Kalau nggak ada uang, bekalmu aja aku ambil!”
> Bima: “Jangan, itu bekal dari rumah.”
> Fatimah: “Aku lihat dia ke arah rak dekat kantin. Aku bantu cari, ya.”
> Aji: “Bim, aku bantu cari barangmu.”


**Kartu Aksi:** A-02 TANYA Fatimah → A-03 PERIKSA lima titik → A-04 AMBIL 2 item.

**Kartu Kata:** K-02 “Menawarkan bantuan” setelah Bima tenang.

**Selesai jika:** Fatimah mendampingi; botol dan bekal ada di inventory quest (2/2).

**Pelajaran:** Mengambil barang tanpa izin dan membuat pemiliknya takut bukan keusilan biasa.

## L1-S05 Mengembalikan barang

Lokasi: Bangku taman \| Tujuan adegan: Dukungan tanpa paksaan

Aji mengembalikan barang. Bima berterima kasih, tetapi masih takut. Pemain menyusun kalimat yang menawarkan dukungan tanpa memaksa laporan segera.

> Aji: “Ini barangmu, Bim. Sudah lengkap?”
> Bima: “Iya. Makasih... tapi aku takut besok diambil lagi.”
> Aji: “Kalau kamu mau, aku bisa temani cerita ke guru.”
> Bima: “Aku pikir dulu, ya.”


**Kartu Aksi:** A-05 KEMBALIKAN + A-09 CATAT kronologi.

**Kartu Kata:** K-03 “Dukungan Bima”.

**Selesai jika:** 2/2 barang diserahkan; Bima menjawab; jurnal pelajaran terbuka.

**Pelajaran:** Dukungan berarti mendengar dan menawarkan bantuan, bukan memaksa korban.

## 5.2 Puzzle lingkungan Level 1 — dua barang dari lima titik

| **Node**                    | **Petunjuk / keadaan**                        | **Hasil inspeksi**                                            |
|-----------------------------|-----------------------------------------------|---------------------------------------------------------------|
| P1 — Meja kantin            | Fatimah melihat Memet melewati sisi meja.     | Keping petunjuk arah; bukan item.                             |
| P2 — Bangku dekat taman     | Terdapat tas tertinggal bukan milik Bima.     | Distraktor aman; jangan ambil barang orang.                   |
| P3 — Rak troli kantin       | Botol biru dengan label Bima tampak terselip. | BOTOL +1; harus minta izin petugas kantin bila di area kerja. |
| P4 — Pot besar dekat lorong | Terlihat bekas langkah menuju bangku pojok.   | Petunjuk lokasi akhir.                                        |
| P5 — Bangku pojok terbuka   | Kotak bekal ada di bawah bangku.              | BEKAL +1.                                                     |

Aturan puzzle: lima titik muncul di area kecil yang dapat dijelajahi. Pemain dapat memeriksa dalam urutan apa pun, tetapi P3 perlu interaksi dengan petugas jika berada di area kerja. Petunjuk dapat menunjukkan arah tanpa memaksa pemain menelusuri semua titik. Jika tersesat lebih dari 90 detik, marker petunjuk aktif. Tidak ada hukuman mengambil objek salah—Aji akan berkata “Ini bukan barang Bima, jangan kuambil.”

# 06. Kartu Kata Level 1 — konten siap implementasi

Format setiap puzzle: 3 slot berurutan + 2 keping pengecoh, kecuali tutorial K-01 yang lebih pendek. Pemain menyusun intent yang benar, lalu Aji mengucapkannya. Keping yang keliru tidak otomatis diucapkan kepada NPC.

## K-01 — Tanya kondisi

Target: Bima \| Pemicu: Memet baru pergi dari koridor

| **Slot 1** | **Slot 2** | **Slot 3**     |
|------------|------------|----------------|
| Bim,       | kamu       | nggak apa-apa? |

Pengecoh: Ah, lebay! • Biarin saja.

Ucapan saat benar: “Bim, kamu nggak apa-apa?”

Feedback salah: Jangan menyepelekan ekspresi teman yang terlihat takut.

Respon NPC / progres: Bima berkata sudah biasa; Aji belum tahu pola bullying. Quest berlanjut ke kelas.

## K-02 — Tawarkan bantuan

Target: Bima \| Pemicu: Barang Bima baru diambil

| **Slot 1**     | **Slot 2**     | **Slot 3**      |
|----------------|----------------|-----------------|
| Aku bisa bantu | cari barangmu, | kalau kamu mau. |

Pengecoh: Kamu salah sendiri. • Harus lawan, dong!

Ucapan saat benar: “Aku bisa bantu cari barangmu, kalau kamu mau.”

Feedback salah: Bantuan sebaiknya ditawarkan, bukan dipaksakan atau menyalahkan korban.

Respon NPC / progres: Bima menyetujui pencarian; Fatimah memberi petunjuk pertama.

## K-03 — Dukung Bima

Target: Bima \| Pemicu: Bekal dan botol telah kembali

| **Slot 1**      | **Slot 2**      | **Slot 3**      |
|-----------------|-----------------|-----------------|
| Kalau kamu mau, | aku bisa temani | cerita ke guru. |

Pengecoh: Lupakan saja semuanya. • Harus sekarang!

Ucapan saat benar: “Kalau kamu mau, aku bisa temani cerita ke guru.”

Feedback salah: Biarkan korban memilih kapan bercerita, sambil tetap mengutamakan keselamatan.

Respon NPC / progres: Bima berkata masih perlu waktu; Aji mencatat kejadian dan informasi bantuan.

> **REFLEKSI AKHIR LEVEL 1**
> “Candaan yang membuat orang takut perlu dihentikan. Mengambil barang teman tanpa izin bukan tindakan yang benar. Mendampingi korban berarti mendengar, membantu secara aman, dan menawarkan orang dewasa yang dapat dipercaya.”


## 6.1 Acceptance criteria Level 1

- AC-L1-01: Player tidak dapat mengaktifkan K-01 sebelum Memet dan Raka meninggalkan koridor.
- AC-L1-02: Dialog awal Aji tidak menyebut Memet sebagai pelaku bullying karena Aji belum tahu riwayat Bima.
- AC-L1-03: Kedua item memiliki ID unik; barang non-target tidak bisa masuk inventory quest.
- AC-L1-04: Progress L1-Q04 tetap 0/2, 1/2, 2/2 saat pemain mengumpulkan barang dalam urutan apa pun.
- AC-L1-05: L1-Q05 tidak selesai sebelum KEMBALIKAN 2/2 dan K-03 tersusun.
- AC-L1-06: Setelah dua jawaban puzzle yang kurang tepat, pemain mendapat hint, bukan game over.
- AC-L1-07: Jurnal akhir bab menyimpan tiga gagasan: “candaan”, “barang pribadi”, “dukungan korban”.

# 07. Level 2 — Luka yang Tak Terlihat

Tujuan naratif: tindakan Memet makin jelas sebagai pola berulang. Bima kembali diganggu; Radit dipaksa mengerjakan tugas dan kacamatanya diambil. Pemain memecahkan puzzle petunjuk di perpustakaan dan menghadapi ancaman dengan menghindar, memilih ucapan tegas yang aman, serta meminta bantuan orang dewasa.

| **Parameter**      | **Target desain**                                                                         |
|--------------------|-------------------------------------------------------------------------------------------|
| Lokasi             | Kantin → kelas VIII A → perpustakaan → gerbang / pos keamanan.                            |
| Durasi target      | 20–30 menit termasuk satu encounter singkat.                                              |
| NPC kunci          | Aji, Bima, Radit, Memet, Raka, Fatimah, pustakawan, petugas.                              |
| Kartu Aksi dominan | TANYA, PERIKSA, AMBIL, KEMBALIKAN, DAMPINGI, MINTA BANTUAN, HINDAR, CATAT.                |
| Kartu Kata dominan | K-04 dukung Radit, K-05 tegas tanpa provokasi, K-06 laporan singkat.                      |
| Exit chapter       | Kacamata kembali, keselamatan Aji/Radit dijaga, ancaman tercatat, bantuan guru dipanggil. |

## 7.1 Alur quest Level 2

| **ID / scene**              | **Objective terlihat player**                        | **Kondisi berhasil**                                                |
|-----------------------------|------------------------------------------------------|---------------------------------------------------------------------|
| L2-Q01 / Kantin             | Bantu Bima saat diganggu lagi.                       | Bima berada di area aman; Fatimah memberi bantuan.                  |
| L2-Q02 / Perpustakaan       | Dengarkan Radit dan temukan 3 petunjuk.              | Petunjuk valid 3/3; lokasi rak diidentifikasi.                      |
| L2-Q03 / Rak sejarah        | Temukan dan kembalikan kacamata.                     | Kacamata ditemukan dengan pendamping pustakawan; Radit menerimanya. |
| L2-Q04 / Pintu perpustakaan | Hadapi ancaman Memet tanpa balasan.                  | Aji menjauh; ancaman dicatat; rute gerbang aman tersedia.           |
| L2-Q05 / Gerbang            | Hindari serangan, susun kalimat batas, cari petugas. | Dodge/safe assist; K-05 selesai; petugas dihubungi.                 |
| L2-Q06 / Pos keamanan       | Sampaikan kronologi dan dukung teman.                | K-06 laporan selesai; checkpoint akhir bab.                         |

## L2-S01 Gangguan berulang

Lokasi: Kantin \| Tujuan adegan: Pola bullying mulai terlihat

Aji, Bima, dan Radit hendak istirahat. Memet kembali meminta uang. Bima terdorong dan Fatimah menghampiri. Aji fokus membantu tanpa membalas.

> Memet: “Bim, traktir aku dong!”
> Bima: “Aku nggak punya uang lebih.”
> Fatimah: “Bim, duduk dulu di sini, yuk.”
> Aji: “Kita cari Bu Ratna supaya aman.”


**Kartu Aksi:** A-06 DAMPINGI Bima dan A-07 MINTA BANTUAN.

**Kartu Kata:** Tidak ada puzzle wajib; dialog naratif singkat.

**Selesai jika:** Bima menjauh dari Memet, berada dekat saksi dan petugas.

**Pelajaran:** Saksi dapat membantu tanpa terlibat perkelahian.

## L2-S02 Kacamata Radit diambil

Lokasi: Perpustakaan \| Tujuan adegan: Konteks teka-teki dan consent

Setelah tugas rangkuman Bu Ratna, Memet memaksa Radit mengerjakan tugasnya dan mengambil kacamatanya. Radit kesulitan melihat dan meminta pertolongan.

> Memet: “Dit, kerjain rangkumanku juga!”
> Radit: “Jangan ambil kacamataku. Aku susah lihat.”
> Aji: “Dit, aku bisa bantu cari. Kita minta bantuan pustakawan juga.”


**Kartu Aksi:** A-02 TANYA Radit; A-07 MINTA BANTUAN pustakawan.

**Kartu Kata:** K-04 “Dukung Radit”.

**Selesai jika:** K-04 terucap dengan benar; petugas mengetahui kehilangan kacamata.

**Pelajaran:** Mengambil alat bantu seseorang dan memaksanya mengerjakan tugas tidak bisa dibenarkan.

## L2-S03 Tiga petunjuk satu lokasi

Lokasi: Rak perpustakaan \| Tujuan adegan: Puzzle investigasi bertingkat

Radit menunjukkan arah samar ke rak sejarah. Pustakawan membantu mencari. Aji memeriksa tiga clue non-rahasia sebelum menentukan lokasi kacamata.

> Radit: “Aku cuma lihat dia lari ke bagian sejarah...”
> Pustakawan: “Ayo cari bersama. Jangan tarik rak atau naik kursi, ya.”
> Aji: “Oke, kita periksa bagian yang aman dijangkau.”


**Kartu Aksi:** A-03 PERIKSA → A-04 AMBIL (setelah izin pendamping) → A-05 KEMBALIKAN.

**Kartu Kata:** Tidak ada puzzle dialog baru; gunakan K-04 sebagai prasyarat.

**Selesai jika:** Tiga clue tercatat, kacamata ditemukan, Radit mengonfirmasi.

**Pelajaran:** Meminta bantuan petugas lebih aman daripada menghadapi pengambil barang sendiri.

## L2-S04 Ancaman setelah kacamata ditemukan

Lokasi: Pintu perpustakaan \| Tujuan adegan: Persiapan konfrontasi

Memet merasa dipermalukan setelah barang kembali. Ia mengancam Aji di dekat pintu perpustakaan. Raka mulai tidak nyaman. Aji menjaga jarak dan memilih menuju gerbang ramai.

> Memet: “Anak baru sok ikut campur. Lihat aja nanti pulang!”
> Raka: “Met, udahlah. Keterlaluan.”
> Aji: “Aku nggak mau ribut. Aku akan cari guru.”


**Kartu Aksi:** A-09 CATAT ancaman; A-06 DAMPINGI Radit ke area aman.

**Kartu Kata:** Kartu K-05 dipersiapkan, belum dimainkan sebelum zona aman.

**Selesai jika:** Rute aman dipilih, Fatimah tahu ancaman.

**Pelajaran:** Ancaman perlu dianggap serius dan ditindaklanjuti, bukan dibalas.

## L2-S05 Konfrontasi tanpa balasan

Lokasi: Gerbang, dekat pos petugas \| Tujuan adegan: Tantangan dodge dan Kartu Kata

Saat Aji berjalan ke arah pos, Memet bergerak mengancam dan mencoba memukul. Adegan sangat singkat, tidak grafis. Player menghindar menuju zona aman; tidak ada tombol serang atau sistem HP. Setelah aman, K-05 muncul.

> Memet: “Berani banget kamu ikut campur!”
> Aji: “Aku nggak mau bertengkar. Tolong berhenti. Aku akan cari guru.”
> Fatimah: “Pak, tolong! Ada siswa yang diancam!”


**Kartu Aksi:** A-08 HINDAR (dua telegraph), A-07 MINTA BANTUAN.

**Kartu Kata:** K-05 “Hentikan konflik dengan aman” di zona aman.

**Selesai jika:** Aji mencapai safe zone; K-05 benar; petugas masuk dan menghentikan insiden.

**Pelajaran:** Menghindar dan mencari bantuan merupakan pilihan aman, bukan tindakan pengecut.

## L2-S06 Catat dan minta tindak lanjut

Lokasi: Pos keamanan / ruang guru \| Tujuan adegan: Penutup edukatif

Petugas dan wali kelas mendengarkan kronologi. Aji dan teman-temannya tidak perlu menghadapi Memet sendirian. Memet belum berubah; sekolah mulai menindaklanjuti.

> Petugas: “Terima kasih sudah lapor. Kalian aman di sini.”
> Aji: “Kacamata Radit diambil, dan tadi aku diancam di gerbang.”
> Bu Ratna: “Kami akan bantu menangani kejadian ini.”


**Kartu Aksi:** A-09 CATAT dan A-07 MINTA BANTUAN.

**Kartu Kata:** K-06 “Laporan singkat”.

**Selesai jika:** Laporan tersimpan; ending Level 2 memicu notifikasi bab berikutnya.

**Pelajaran:** Melapor adalah cara melindungi, bukan mencari balas dendam.

## 7.2 Puzzle perpustakaan — aturan petunjuk

| **Clue** | **Sumber**       | **Konten**                                 | **Fungsi**                         |
|----------|------------------|--------------------------------------------|------------------------------------|
| C1       | Radit            | “Memet lari ke arah rak sejarah.”          | Mempersempit zona dari tiga rak.   |
| C2       | Jejak lingkungan | Satu buku sejarah sedikit tertarik keluar. | Menunjuk subrak yang relevan.      |
| C3       | Pustakawan       | “Ada kotak kecil bergeser di rak bawah.”   | Menunjuk titik inspeksi yang aman. |

Lokasi kacamata ditetapkan di celah rak bawah sejarah (accessible, tanpa memindahkan rak besar). Pemain boleh memilih rak salah; pemeriksaan memberi feedback, bukan penalti. Dengan 2/3 clue, sistem membolehkan menebak; dengan 3/3, marker lokasi terlihat jelas. Item baru dapat diambil ketika pustakawan mendampingi.

# 08. Kartu Kata Level 2 — konten siap implementasi

## K-04 — Dukung Radit

Target: Radit \| Pemicu: Radit kehilangan kacamata

| **Slot 1**     | **Slot 2**  | **Slot 3**                         |
|----------------|-------------|------------------------------------|
| Aku bantu cari | kacamatamu, | dan kita minta bantuan pustakawan. |

Pengecoh: Kerjain saja tugas Memet. • Cari sendiri, ya.

Ucapan saat benar: “Aku bantu cari kacamatamu, dan kita minta bantuan pustakawan.”

Feedback salah: Utamakan bantuan konkret dan keselamatan; jangan menyalahkan Radit.

Respon NPC / progres: Radit menunjukkan arah yang ia ingat; pustakawan membantu.

## K-05 — Tegas tanpa memprovokasi

Target: Memet \| Pemicu: Aji telah mencapai safe zone

| **Slot 1**                | **Slot 2**       | **Slot 3**          |
|---------------------------|------------------|---------------------|
| Aku nggak mau bertengkar. | Tolong berhenti. | Aku akan cari guru. |

Pengecoh: Ayo, kita lawan! • Kamu pasti takut!

Ucapan saat benar: “Aku nggak mau bertengkar. Tolong berhenti. Aku akan cari guru.”

Feedback salah: Jangan mengancam balik. Saat situasi tidak aman, tujuan utamanya menjauh dan meminta bantuan.

Respon NPC / progres: Petugas datang; Memet tidak langsung sadar atau berubah; penanganan berlanjut.

## K-06 — Laporan singkat

Target: Petugas / Bu Ratna \| Pemicu: Saat Aji berada di pos aman

| **Slot 1**              | **Slot 2**              | **Slot 3**          |
|-------------------------|-------------------------|---------------------|
| Kacamata Radit diambil, | aku diancam di gerbang, | kami perlu bantuan. |

Pengecoh: Nggak ada masalah. • Biar kami balas sendiri.

Ucapan saat benar: “Kacamata Radit diambil, aku diancam di gerbang, kami perlu bantuan.”

Feedback salah: Laporan cukup mencakup kejadian, lokasi, dan kebutuhan bantuan.

Respon NPC / progres: Laporan tersimpan di jurnal; guru menyiapkan tindak lanjut bab berikutnya.

## 8.1 Konfrontasi: spesifikasi desain aman

| **Parameter**     | **Nilai usulan untuk prototipe**                                                                     |
|-------------------|------------------------------------------------------------------------------------------------------|
| Panjang encounter | 15–25 detik maksimum; tidak diperpanjang untuk grinding.                                             |
| Telegraph         | Memet memberi pose persiapan 0,8–1,2 detik sebelum gerak; indikator visual + suara opsional.         |
| Serangan          | Dua upaya pukulan tangan kosong yang tidak grafis dan tidak dapat dibalas.                           |
| Input pemain      | Space + arah gerak untuk dodge, lalu bergerak menuju safe zone.                                      |
| Zona aman         | Area dekat petugas gerbang; aktif segera setelah Aji menjauh.                                        |
| Puzzle dialog     | K-05 baru muncul setelah player berada di luar jangkauan dan keadaan terkendali.                     |
| Soft fail         | Jika terkena/terlambat: layar fade cepat → checkpoint aman; tanpa animasi menyakiti yang berlebihan. |
| Assist Mode       | Auto-dodge atau durasi telegraph diperpanjang; syarat belajar tetap tercapai.                        |
| Larangan desain   | Tidak ada serangan balasan, combo, KO, reward meninju, atau penghinaan pelaku.                       |

## 8.2 Acceptance criteria Level 2

- AC-L2-01: Kacamata tidak muncul sebagai target collectible sebelum Radit memberi informasi kehilangan.
- AC-L2-02: 3 clue dapat dicatat tanpa urutan wajib; lokasi rak dapat diidentifikasi dari gabungan informasi.
- AC-L2-03: A-04 AMBIL kacamata aktif hanya jika sudah ditemukan dan ada izin/pendamping petugas.
- AC-L2-04: K-04 memerlukan maksud menawarkan bantuan + meminta pendamping, bukan menyuruh Radit melawan.
- AC-L2-05: Tombol attack tidak ada di seluruh build; Space hanya menghindar pada encounter.
- AC-L2-06: K-05 terkunci ketika Aji masih di area bahaya.
- AC-L2-07: Bila gagal dodge, encounter tetap berakhir aman melalui checkpoint, bukan health = 0.
- AC-L2-08: Level 2 hanya complete jika K-06 dan permintaan bantuan tercatat.

> **REFLEKSI AKHIR LEVEL 2**
> “Mengambil alat bantu teman, memaksa mengerjakan tugas, dan mengancam adalah tindakan yang salah. Kita bisa mendampingi korban, menghindari kekerasan, menetapkan batas dengan tenang, serta menghubungi orang dewasa yang dipercaya.”


# 09. Matriks kebutuhan fungsional

| **ID** | **Prioritas** | **Deskripsi**                                         | **Bukti penerimaan**                                                      |
|--------|---------------|-------------------------------------------------------|---------------------------------------------------------------------------|
| FR-01  | P0            | Quest tracker dan jurnal menyimpan state per level.   | Tahap bertambah secara urut dan bertahan setelah save/load.               |
| FR-02  | P0            | Kartu Aksi hanya aktif menurut objek/NPC dan kondisi. | Kartu tidak relevan disable beserta alasan.                               |
| FR-03  | P0            | Pencarian barang Bima: 2 dari 5 titik.                | Kedua item dapat ditemukan dengan petunjuk; salah tidak mengambil barang. |
| FR-04  | P0            | Puzzle Kartu Kata dengan keping frasa + pengecoh.     | Frasa bisa dipilih/drag; feedback dan retry bekerja.                      |
| FR-05  | P0            | Puzzle tiga clue kacamata Radit.                      | Clue tersimpan dan unlock inspeksi rak sejarah.                           |
| FR-06  | P0            | Pengembalian item dengan konfirmasi pemilik.          | State item berpindah inventory → NPC dan quest selesai.                   |
| FR-07  | P0            | Konfrontasi dodge tanpa menyerang balik.              | Dua telegraph, safe zone, fail-safe, tidak ada attack.                    |
| FR-08  | P0            | Meminta bantuan petugas pada insiden serius.          | NPC petugas merespons, laporan masuk jurnal.                              |
| FR-09  | P0            | Autosave setiap scene / objective penting.            | Load kembali tanpa hilang progres/duplikasi item.                         |
| FR-10  | P1            | Hint adaptif puzzle setelah beberapa kegagalan.       | Hint muncul setelah 2 gagal dan auto-solve setelah 4.                     |
| FR-11  | P1            | Pengaturan font, subtitle, assist dodge.              | Player dapat menamatkan kedua level via assist.                           |
| FR-12  | P1            | Review pelajaran di akhir level.                      | 3 poin edukasi muncul dan bisa dibaca ulang.                              |
| FR-13  | P2            | Ikon kartu animasi sederhana serta voice-over.        | Tidak menghambat prototipe inti bila belum ada.                           |

## 09.1 Persyaratan nonfungsional

| **Kategori**     | **Target awal (perlu diuji pada perangkat sasaran)**                                                              |
|------------------|-------------------------------------------------------------------------------------------------------------------|
| Kinerja          | 60 FPS target di PC menengah; mode 30 FPS tetap nyaman untuk prototipe.                                           |
| Waktu respons    | Perubahan UI dan kartu setelah interaksi terasa langsung (target \< 200 ms).                                      |
| Keterbacaan      | UI kontras terhadap scene sekolah; font dapat diubah; teks tidak tertutup oleh HUD.                               |
| Penyimpanan      | Autosave antar scene, puzzle state, dan checkpoint.                                                               |
| Privasi evaluasi | Tidak mengambil identitas siswa secara default; jika ada studi, data peserta dianonimkan dan gunakan persetujuan. |
| Keselamatan isi  | Tidak ada penghargaan atas serangan fisik; korban tidak dipaksa memaafkan.                                        |

# 10. State machine dan arsitektur Unity

## 10.1 Sistem teknis yang dibutuhkan

| **Komponen**         | **Tanggung jawab**                                                         |
|----------------------|----------------------------------------------------------------------------|
| QuestManager         | CurrentObjective, objective flags, chapter unlock, save progress.          |
| CardManager          | Menentukan Kartu Aksi tersedia dan puzzle Kartu Kata yang terpicu.         |
| ActionResolver       | Cek jarak, target, ownership/permission, active quest, lalu terapkan aksi. |
| WordPuzzleController | Load keping/slot/distraktor, cek intent, hint, retry, emit WordSolved.     |
| ClueJournal          | Menyimpan clue, sumber, barang 0/2, dan kronologi ringan.                  |
| DialogueController   | Dialog NPC, intent safe, callback animasi, subtitle.                       |
| EncounterController  | Telegraph, dodge, safe zone, soft fail, assist mode.                       |
| SaveGameManager      | Persist flags quest, kartu unlocked, item, clue, checkpoint.               |

## 10.2 Model data disarankan

| **Entity / field**   | **Contoh**                                                                                          |
|----------------------|-----------------------------------------------------------------------------------------------------|
| ActionCardDefinition | id, title, icon, triggerCondition, allowedTargetType, repeatable, actionType                        |
| WordPuzzleDefinition | id, contextNPC, slotIntentIds\[\], distractorIds\[\], hint, successLine, safeTags\[\]               |
| QuestObjective       | id, chapterId, prerequisites\[\], requiredEvents\[\], completeEvent                                 |
| RuntimeState         | unlockedActionCards\[\], solvedWordCards\[\], foundClues\[\], questItems\[\], checkpointId          |
| Event contoh         | OnWitnessSpoken, OnItemInspected, OnQuestItemFound, OnWordSolved, OnSafeZoneReached, OnAdultAlerted |

## 10.3 Pseudocode validasi (ringkas)

IF target is accessible AND card context matches current quest: execute ActionCard → emit event → update objective.  
IF WordPuzzle contains all required intent IDs in valid order: play Aji dialogue → emit OnWordSolved; ELSE show feedback → retry.  
IF Encounter state == SAFE_ZONE: unlock K-05; IF adult alerted AND K-05 solved: close encounter safely.

# 11. Evaluasi, QA, dan risiko produk

## 11.1 Uji tugas akhir (indikator dan metode)

| **Hipotesis desain**                       | **Metode ukur**                          | **Target awal, bukan hasil**                                           |
|--------------------------------------------|------------------------------------------|------------------------------------------------------------------------|
| Pemain memahami fungsi dua kartu.          | Observasi + pertanyaan sesudah tutorial. | ≥80% dapat menjelaskan perbedaannya.                                   |
| Pemain dapat menuntaskan pencarian barang. | Log completion + hint usage.             | ≥75% menyelesaikan L1-Q04 tanpa bantuan fasilitator.                   |
| Pemain memilih dukungan aman.              | Pre/post 5 soal skenario.                | Peningkatan rata-rata pemahaman; target ditetapkan dalam desain studi. |
| Dodge tidak menghambat edukasi.            | Tingkat retry + assist use + keluhan.    | ≥90% dapat lanjut setelah maksimal 3 percobaan / assist.               |
| Dialog mudah dipahami.                     | Think-aloud / SUS sederhana / interview. | Temuan dipakai untuk revisi phrasing.                                  |

## 11.2 Skenario QA inti

- QA-01: Menekan E jauh dari petunjuk tidak mengaktifkan aksi atau mengubah quest.
- QA-02: Item yang bukan milik Bima tidak dapat diambil; pesan singkat muncul.
- QA-03: Menyusun keping Kartu Kata dalam urutan salah lalu mengulang tidak menggandakan progress.
- QA-04: Save/load ketika botol ditemukan namun bekal belum ditemukan menghasilkan 1/2, bukan reset.
- QA-05: Radit tidak bisa diberi kacamata sebelum item sah berada di inventory.
- QA-06: Pergi ke pintu perpustakaan tanpa bicara pustakawan tidak menyelesaikan L2-Q03.
- QA-07: Player masuk safe zone tanpa dodge karena Assist Mode tetap mendapat pelajaran yang sama.
- QA-08: Konfrontasi tidak pernah menampilkan perintah “pukul”, “serang”, atau reward damage.
- QA-09: Setiap ending bab tetap dapat diakses melalui jurnal setelah save/load.

## 11.3 Risiko dan mitigasi

| **Risiko**                               | **Mitigasi**                                                                |
|------------------------------------------|-----------------------------------------------------------------------------|
| Kartu Kata terasa seperti soal pelajaran | Ikatan kuat pada dialog karakter, keping singkat, respons NPC terasa nyata. |
| Exploration membingungkan                | Hotspot 5 titik, compass ringan, hint bertahap, clue dari saksi.            |
| Konfrontasi menormalisasi kekerasan      | Batas durasi, dodge tanpa attack, safe zone, petugas menghentikan kejadian. |
| Korban terkesan disalahkan               | Semua intent dukungan ditinjau; tidak ada label “korban lemah/bersalah”.    |
| Memet berubah instan                     | Ending bab 2 hanya ada bantuan; proses perubahan di bab lanjut.             |
| Scope terlalu berat untuk TA             | Prioritaskan vertical slice L1-Q04 → L1-Q05 dan L2-Q02 → L2-Q05.            |

## 11.4 Prioritas pengembangan / sprint

| **Sprint**           | **Output minimum yang bisa didemokan**                                 |
|----------------------|------------------------------------------------------------------------|
| 1 — Fondasi          | Third-person, kamera, NPC interact, quest tracker, Kartu Aksi generik. |
| 2 — Level 1 inti     | L1-Q04 5 titik, dua item, inventory, Kartu Kata K-02/K-03.             |
| 3 — Level 2 inti     | Kacamata 3 clue, Kartu Kata K-04, pustakawan dan return item.          |
| 4 — Konflik aman     | Encounter 2 telegraph, dodge, safe zone, K-05, petugas.                |
| 5 — Polishing & test | Autosave, hint, aksesibilitas, edukasi akhir bab, playtesting.         |

# 12. Definisi selesai (Definition of Done)

- Dua level dapat dimainkan dari awal sampai akhir tanpa deadlock quest atau kehilangan item.
- Setiap level memiliki minimal dua penggunaan Kartu Aksi dan dua puzzle Kartu Kata yang terkait langsung dengan konflik cerita.
- Pemain menerima feedback eksplisit tentang mengapa sebuah kalimat aman atau tidak tepat, tanpa memaksa korban memaafkan.
- Kondisi menang pada adegan ancaman adalah selamat dan mendapat bantuan, tidak pernah mengalahkan pelaku dengan kekerasan.
- Masing-masing level menampilkan rangkuman anti-bullying dan menyimpan pelajaran di jurnal.
- Penguji dapat melihat log quest, kartu, dan pengukuran dasar untuk laporan Tugas Akhir.

> **KEPUTUSAN DESAIN YANG PERLU DIPATOK SEBELUM PRODUKSI**
> Tentukan istilah final HUD “Kartu Aksi” & “Kartu Kata”; Quest Tracker sebagai menu misi. Kunci jenis puzzle frasa 3-slot dan pemisahan dialog untuk korban vs pelaku. Setelah disetujui, barulah authoring visual UI, animasi, dan implementasi data-driven dilakukan.


## Catatan sumber dan status dokumen

Alur karakter, lokasi, dan konflik level 1–2 mengikuti revisi “UNSEEN_SIDE_Narasi_5_Chapter(1).docx” serta “UNSEEN_SIDE_GDD_Kartu_Quest_dan_Aksi_V2.pptx” yang dibahas sebelumnya. Desain Kartu Kata, ID fitur, durasi target, aturan puzzle, assist mode, acceptance criteria, dan target uji adalah usulan baru dalam PRD ini—belum merupakan hasil implementasi atau pengujian.
