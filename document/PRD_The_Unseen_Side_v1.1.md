# PRD The Unseen Side

Product Requirements Document

## Daftar isi

- [1 Tujuan produk dan pengguna](#1-tujuan-produk-dan-pengguna)
- [2 Hasil belajar dan prinsip penyampaian](#2-hasil-belajar-dan-prinsip-penyampaian)
- [3 Ruang lingkup dan indikator keberhasilan](#3-ruang-lingkup-dan-indikator-keberhasilan)
- [4 Premis cerita dan peran karakter](#4-premis-cerita-dan-peran-karakter)
- [5 Lingkungan sekolah dan rancangan level](#5-lingkungan-sekolah-dan-rancangan-level)
- [6 Alur pemain dan mekanik inti](#6-alur-pemain-dan-mekanik-inti)
- [7 Chapter 1 Hari Pertama dan Sisi yang Tak Terlihat](#7-chapter-1-hari-pertama-dan-sisi-yang-tak-terlihat)
- [8 Chapter 2 Di Balik Tawa di Lapangan](#8-chapter-2-di-balik-tawa-di-lapangan)
- [9 Chapter 3 Bangku yang Kosong](#9-chapter-3-bangku-yang-kosong)
- [10 Chapter 4 Tekanan di Balik Tugas](#10-chapter-4-tekanan-di-balik-tugas)
- [11 Chapter 5 Pemulihan dan Perubahan](#11-chapter-5-pemulihan-dan-perubahan)
- [12 Kebutuhan fungsional dasar](#12-kebutuhan-fungsional-dasar)
- [13 Dialog dan perlindungan dalam gameplay](#13-dialog-dan-perlindungan-dalam-gameplay)
- [14 Antarmuka dan aksesibilitas](#14-antarmuka-dan-aksesibilitas)
- [15 Arah visual audio dan daftar aset](#15-arah-visual-audio-dan-daftar-aset)
- [16 Struktur proyek Unity arsitektur dan penyimpanan](#16-struktur-proyek-unity-arsitektur-dan-penyimpanan)
- [17 Kebutuhan nonfungsional dan build](#17-kebutuhan-nonfungsional-dan-build)
- [18 Rencana pengujian fungsional](#18-rencana-pengujian-fungsional)
- [19 Evaluasi isi usability dan pembelajaran](#19-evaluasi-isi-usability-dan-pembelajaran)
- [20 Tahap pengembangan dan prioritas backlog](#20-tahap-pengembangan-dan-prioritas-backlog)
- [21 Risiko dan keputusan implementasi](#21-risiko-dan-keputusan-implementasi)
- [22 Ketertelusuran dan definisi selesai](#22-ketertelusuran-dan-definisi-selesai)
- [23 Dasar rancangan dan istilah](#23-dasar-rancangan-dan-istilah)

**Game edukasi perundungan untuk siswa SMP**

Versi 1.1  |  19 Oktober 2026  |  Acuan perancangan dan implementasi tugas akhir

## Ringkasan produk

The Unseen Side adalah game petualangan naratif 3D yang menempatkan pemain sebagai Aji, siswa pindahan yang belajar mengenali perundungan dan memberi bantuan yang aman. Melalui lima chapter dan lima belas misi, pemain mendampingi tiga teman, melibatkan guru, lalu memeriksa kembali rasa aman mereka setelah penanganan.

PRD ini menetapkan perilaku produk, ruang lingkup, kebutuhan sistem, kriteria penerimaan, dan rencana pengujian. Dokumen ini digunakan bersama GDD, naskah dialog, dan rancangan level untuk membangun versi tugas akhir yang dapat dimainkan dari awal sampai akhir.

| Elemen | Ketentuan produk |
| --- | --- |
| Pemain utama | Siswa SMP usia sekitar 12–15 tahun; bahasa Indonesia. |
| Format | Single-player, offline, Windows PC; dikembangkan dengan Unity. |
| Penyajian | 3D third-person; kamera mengikuti Aji dan dapat diputar. |
| Gaya visual | Smooth low-poly; proporsi badan terhadap kepala 2:1. |
| Konten inti | Bima: ejekan tubuh; Laras: pengucilan terarah; Raka: pemaksaan PR dan penyembunyian tas. |
| Hasil yang dituju | Pemain dapat memilih dukungan saksi yang aman dan mengetahui peran guru dalam perlindungan serta tindak lanjut. |

## Status kebutuhan

Struktur cerita lima chapter dan lima belas misi menjadi dasar produk. Durasi, jadwal, spesifikasi perangkat, dan ambang evaluasi yang diberi label usulan adalah target perencanaan; pencapaiannya harus dicatat melalui uji, bukan dianggap sebagai hasil yang sudah terbukti.

**Pembaruan versi 1.1:** struktur implementasi mengikuti folder proyek Unity pada tangkapan layar, dengan root `Assets/_Game/`. Chapter 1–5 dipetakan ke `Level_01`–`Level_05`. Penempatan aset, scene, data dan skrip dijelaskan pada Bagian 16. Susunan cerita dan lima belas misi tetap menjadi acuan produk.


## 1 Tujuan produk dan pengguna

### Masalah yang ingin dijawab

Situasi perundungan di sekolah dapat tampak seperti candaan, urusan pertemanan, atau saling membantu tugas. Pemain perlu berlatih membedakan apa yang benar-benar diamati, memahami pengalaman teman, dan memilih langkah aman tanpa mengambil alih tanggung jawab penanganan sekolah.

Produk menyediakan latihan berbasis skenario. Keberhasilannya dinilai melalui keterpahaman, kemampuan menyelesaikan interaksi, dan jawaban atas situasi pembelajaran. Game tidak memberi diagnosis kondisi keluarga atau menjanjikan penghapusan perundungan di dunia nyata.

| Pengguna | Kebutuhan | Implikasi produk |
| --- | --- | --- |
| Siswa pemula | Dapat bergerak dan memahami tugas tanpa pengalaman game 3D. | Tutorial bertahap; penanda tujuan, teks sederhana, bantuan kontrol. |
| Siswa yang pernah mengalami kejadian serupa | Dapat menjaga kenyamanan selama bermain. | Jeda dan keluar kapan saja; adegan singkat; tidak dipaksa bercerita pengalaman pribadi. |
| Guru atau fasilitator | Dapat mengajak refleksi setelah sesi. | Ringkasan pesan per chapter dan pertanyaan situasional; tanpa akun guru pada versi inti. |
| Pengembang TA dan pembimbing | Dapat memeriksa keterlaksanaan serta ketepatan pesan. | Kebutuhan ber-ID, matriks pengujian, naskah yang dapat ditinjau sebelum produksi. |

### Tujuan produk

- Menyediakan satu permainan utuh dengan tiga kasus yang saling terhubung di lingkungan sekolah SMP Indonesia.

- Memungkinkan pemain mencoba respons, menerima penjelasan, lalu memperbaiki pilihan tanpa menghukum korban.

- Menempatkan guru sebagai pihak yang menghentikan perilaku, melindungi siswa, dan melakukan tindak lanjut.

- Menjaga cakupan implementasi agar lima belas misi dapat diselesaikan dan diuji dalam proyek TA.

### Konteks pemakaian

Usulan durasi bermain pertama adalah 60–90 menit, dapat dibagi menjadi beberapa sesi melalui checkpoint. Durasi akhir ditetapkan setelah playtest; siswa tidak wajib menuntaskan permainan dalam satu jam pelajaran. Pemakaian terbimbing dapat menambahkan diskusi di luar game.

## 2 Hasil belajar dan prinsip penyampaian

Hasil belajar dirumuskan sebagai tindakan yang dapat diperiksa melalui pilihan skenario. Kode LP digunakan untuk menghubungkan pesan pembelajaran, misi, dan evaluasi.

| Kode | Setelah bermain siswa diharapkan dapat | Bukti dalam permainan |
| --- | --- | --- |
| LP01 | Mengenali indikasi perundungan dan mencari konteks pengulangan serta ketimpangan kuasa. | M2 membedakan fakta dan dugaan; M4, M7, M10 melengkapi konteks. |
| LP02 | Menjelaskan bahwa tawa canggung atau diam tidak otomatis berarti nyaman. | Percakapan Bima pada M4 dan soal refleksi. |
| LP03 | Memberi dukungan tanpa memaksa korban bercerita atau menerima ajakan. | Alternatif respons pada M3, M8, dan M14. |
| LP04 | Memilih bantuan aman dan menyampaikan informasi yang diketahui kepada guru. | M5–6, M9, M11–12; jalur bantuan tetap tersedia. |
| LP05 | Memahami pemulihan dan tindak lanjut, termasuk risiko intimidasi lanjutan. | Pemeriksaan terpisah pada M14 dan penutup M15. |
| LP06 | Memahami latar keluarga pelaku tanpa membenarkan perilaku atau menyalahkan korban. | Cutscene M13 dan refleksi tentang tanggung jawab pelaku. |

### Aturan isi yang wajib dijaga

- Satu insiden awal atau ekspresi wajah tidak cukup untuk menyimpulkan seluruh pola. Ketidaknyamanan tetap boleh ditolong sejak awal, tanpa menunggu label perundungan pasti.

- Perbedaan tubuh, kemampuan, atau pertemanan tidak menjadi kesalahan korban. Bima tidak harus menurunkan berat badan; Laras tidak harus menjadi populer.

- Pemaksaan PR dan penyembunyian tas ditampilkan sebagai tekanan dan gangguan terhadap barang. Tidak diperlukan adegan kekerasan berat.

- Respons tidak aman menerima umpan balik dan kesempatan mencoba lagi. Game tidak memperpanjang penderitaan korban sebagai hukuman pemain.

- Keluarga Dion adalah latar fiksi yang berkontribusi pada konflik. Tidak semua anak dengan masalah keluarga melakukan perundungan.

- Permintaan maaf tidak otomatis mengakhiri kasus. Korban boleh belum memaafkan; sekolah tetap bertanggung jawab memantau keamanan.

## 3 Ruang lingkup dan indikator keberhasilan

Prioritas Must wajib ada pada versi akhir TA. Should dikerjakan setelah alur inti stabil; Could hanya jika tidak mengganggu pengujian. Vertical slice adalah tahap produksi, bukan pengurangan lima chapter pada produk akhir.

| Prioritas | Cakupan |
| --- | --- |
| Must | 5 chapter dan 15 misi; 3 kasus; seluruh respons aman dan tindak lanjut guru; ending reflektif. |
| Must | Gerak Aji, kamera third-person, interaksi, dialog bercabang lokal, tujuan misi, jurnal, peta sederhana, jeda, pengaturan, checkpoint dan continue. |
| Must | Kelas, lapangan olahraga, koridor, ruang guru/BK; semua tokoh inti hadir sesuai peran, termasuk keluarga Aji melalui adegan singkat. |
| Must | Teks bahasa Indonesia, subtitle seluruh ujaran bersuara, penanda selain warna, bantuan kontrol dan pengaturan sensitivitas kamera. |
| Should | Pemilihan chapter selesai untuk bermain ulang; penyesuaian tombol; log pengujian lokal anonim dengan persetujuan peserta. |
| Could | Dukungan controller, voice-over lengkap, ruang kantin/perpustakaan yang bisa dijelajahi, animasi wajah lebih rinci. |
| Di luar versi inti | Multiplayer, co-op, split-screen, combat, open world luas, akun/cloud save, media sosial dalam game, sistem disiplin sekolah daring. |

### Target evaluasi produk yang diusulkan

| Indikator | Target rilis dan cara memeriksa |
| --- | --- |
| Kelengkapan | 15 dari 15 misi dapat dituntaskan pada build uji; tidak ada syarat yang saling mengunci. |
| Stabilitas | Tidak ada bug yang menghilangkan progres, menghentikan alur, atau melanggar pesan perlindungan pada daftar uji wajib. |
| Kemudahan | Usulan ≥80% peserta uji dapat menyelesaikan tugas navigasi dan bantuan utama tanpa arahan langsung; laporkan jumlah peserta dan konteks uji. |
| Pemahaman | Usulan ≥80% jawaban benar pada butir inti sesudah bermain; bandingkan juga perubahan tiap indikator dengan pretest. |
| Validitas isi | Naskah dan skenario ditinjau pembimbing serta guru BK atau penelaah kompeten; temuan kritis diselesaikan sebelum uji siswa. |

Ambang di atas bukan hasil penelitian atau batas baku. Pembimbing menetapkan rancangan evaluasi dan kelayakan target sebelum pengambilan data. Jika target tidak tercapai, dokumentasikan hasil serta perbaikannya.

## 4 Premis cerita dan peran karakter

Aji mengikuti orang tuanya pindah rumah sehingga harus berpindah sekolah. Ia dibesarkan dalam keluarga yang terbiasa mendengar dan membahas masalah dengan tenang. Kenangan beberapa ejekan di sekolah lama membuatnya peka, tetapi ia tetap ragu dan belajar meminta bantuan Sari serta guru.

| Tokoh | Peran dan latar | Kebutuhan penyajian |
| --- | --- | --- |
| Aji | Protagonis; siswa baru yang peduli dan masih belajar. | Pemain mengendalikan Aji; tidak menyelesaikan semua konflik sendirian. |
| Sari | Pendamping orientasi dan saksi. | Memberi petunjuk singkat; tidak menjadi mesin jawaban setiap pilihan. |
| Bima | Mengalami ejekan tubuh dan tertawa canggung. | Memiliki minat berolahraga; boleh memilih kapan kembali ke lapangan. |
| Laras | Mengalami penolakan kelompok atas hasutan Dion. | Memiliki pendapat dan pilihan; kasus utama bukan kesulitan membaca. |
| Raka | Ditekan menyerahkan jawaban PR; tasnya disembunyikan. | Dapat menolak, meminta bantuan, dan mendapatkan kembali kendali atas tugas serta barangnya. |
| Dion | Pelaku utama; komunikasi merendahkan di rumah dan penguatan teman. | Latar muncul bertahap; tetap bertanggung jawab menghentikan dan memperbaiki perbuatan. |
| Tio | Menguatkan ejekan melalui tawa dan dukungan. | Ditangani atas perilakunya sendiri; belajar berhenti memberi dukungan. |
| Fajar | Menyembunyikan tas dan mendukung tekanan. | Mengembalikan barang dan mengikuti tindak lanjut sekolah. |
| Pak Beni | Guru PJOK. | Menghentikan komentar tubuh dan membagi tim secara adil. |
| Bu Ratih | Wali kelas atau guru kelas. | Memeriksa pengucilan dan mengatur kelompok belajar. |
| Pak Arif | Guru BK. | Menjaga privasi, menyusun perlindungan, dan memantau tindak lanjut. |
| Ayah dan Ibu Aji | Dua tokoh pendukung dalam prolog singkat. | Menunjukkan kebiasaan mendengar dan keberanian meminta bantuan. |

Total pemeran inti: 8 siswa dan 5 orang dewasa. Siswa latar dapat memakai variasi model dasar tanpa tambahan alur personal. Detail konseling keluarga pelaku hanya muncul melalui cutscene terpisah untuk pemain, tidak melalui penyadapan oleh Aji.

## 5 Lingkungan sekolah dan rancangan level

Sekolah dibangun sebagai satu kawasan kompak yang dapat digunakan kembali pada kelima chapter. Ukuran ruang ditentukan melalui blockout kamera dan jalur berjalan, sebelum detail aset dibuat. Tata letak harus menjelaskan hubungan ruang belajar, lapangan, koridor, dan akses bantuan guru.

| Area wajib | Fungsi cerita | Kriteria penerimaan level |
| --- | --- | --- |
| Ruang kelas | Orientasi; pengucilan Laras; pemaksaan tugas; refleksi. | Meja, kursi, papan tulis, lemari, meja guru, pintu dan jendela terbaca; jalur Aji tidak tersumbat NPC. |
| Lapangan olahraga | Ejekan tubuh Bima; bantuan Pak Beni; follow-up. | Area bermain, tepi lapangan untuk percakapan, pembagian tim dan posisi guru dapat dibedakan. |
| Koridor | Penghubung ruang; percakapan singkat; rute tenang ke BK. | Arah ruang ditandai; tersedia jalur biasa dan pilihan rute lebih tenang pada M12. |
| Ruang guru | Akses bantuan orang dewasa dan pencarian tas. | Titik interaksi guru mudah ditemukan; penanda bantuan tersedia saat diperlukan. |
| Ruang BK | Pelaporan privat dan tindak lanjut. | Percakapan tidak menjadi tontonan; NPC tidak berkumpul menghalangi pintu; tersedia titik masuk/keluar aman. |
| Area prolog | Percakapan Aji dan orang tua. | Dapat berupa set kecil atau cutscene ringkas; tidak membutuhkan rumah yang bisa dijelajahi. |

### Aturan perpindahan dan visibilitas

- Peta sederhana menampilkan nama area, posisi Aji, dan tujuan aktif yang sesuai pengetahuan pemain. Tidak menampilkan posisi rahasia korban atau isi konseling.

- Pintu dan belokan memiliki ruang cukup untuk kamera serta interaksi. Kamera tidak menembus dinding atau membuat pemain kehilangan arah saat keluar kelas.

- Perubahan jam pelajaran dilakukan melalui transisi chapter atau cutscene, sehingga tidak memerlukan simulasi jadwal seluruh sekolah.

- Area terkunci memberi alasan singkat dan arah tujuan. Ruang guru/BK tetap dapat diakses untuk bantuan kontekstual meskipun laporan kasus lengkap belum dimulai.

### Pembatasan aset lingkungan

Gerbang, kantin, dan perpustakaan boleh menjadi latar atau tambahan setelah jalur inti selesai. Tidak ada kebutuhan membangun seluruh kompleks sekolah. Prop yang hanya dekoratif harus dapat dibedakan dari objek yang memiliki prompt interaksi.

## 6 Alur pemain dan mekanik inti

### Perjalanan dari menu sampai epilog

Pemain membuka game, mengatur tampilan dan audio, memilih mulai baru atau lanjutkan, mengikuti tutorial bersama Sari, menyelesaikan misi per chapter, mengakses ringkasan pembelajaran, lalu mencapai refleksi dan epilog. Menu jeda serta penyimpanan checkpoint memungkinkan sesi dilanjutkan di lain waktu.

### Siklus interaksi

Menjelajah area → mengamati kejadian → mendengar pengalaman teman → memilih dukungan → meminta bantuan guru → melihat tindak lanjut. Urutan cerita utama tetap terarah; dialog dan jalur pendampingan dapat memiliki alternatif yang kembali ke titik kemajuan yang sama.

| Mekanik | Perilaku yang dibutuhkan |
| --- | --- |
| Observasi | Pemain mendekati kejadian dan memicu adegan singkat. Jurnal membedakan hal yang dilihat, cerita yang didengar, dan hal yang belum diketahui. |
| Percakapan | Pilihan 2–3 respons pendek; ada pilihan memberi ruang atau meminta pendamping. Dialog dapat dibaca ulang lewat ringkasan yang tidak membuka rahasia. |
| Dukungan | Ajak bicara secara pribadi, temani bila diinginkan, tawarkan kelompok, atau minta bantuan guru. Tidak ada tombol memaksa korban menerima. |
| Pilihan kurang aman | Tampilkan alasan singkat, lalu kembali ke pilihan. Hinaan atau perkelahian tidak menjadi minigame maupun adegan yang diberi hadiah. |
| Jurnal | Catatan berbasis fakta cerita; tidak ada foto, perekaman tersembunyi, atau kewajiban mengumpulkan seluruh bukti. |
| Bantuan kontekstual | Sari atau petunjuk tujuan membantu pemain yang tersesat. Bantuan guru dapat diminta tanpa menunggu semua subtujuan selesai. |

### Kemajuan dan kondisi selesai

Misi menggunakan status Locked, Available, Active, dan Completed. Setiap misi memiliki subtujuan dan checkpoint. Pemicu harus idempoten: interaksi berulang tidak menggandakan barang, hasil refleksi, atau transisi. Status yang terhalang karena NPC sedang berpindah harus pulih melalui penempatan ulang yang aman.

Tidak ada skor keberanian, tingkat kepercayaan korban yang wajib penuh, atau game over karena ketahuan melapor. Penghargaan berupa progres, pemahaman, dan hasil dukungan yang terlihat. Keberhasilan tidak ditentukan oleh seberapa cepat korban membuka cerita.

## 7 Chapter 1 Hari Pertama dan Sisi yang Tak Terlihat

Tujuan: mengenal sekolah, mengamati indikasi awal tiga kasus, dan memberi pertolongan ringan. Lokasi: kelas, lapangan, koridor, ruang guru/BK. Target durasi usulan 10–15 menit.

### M01 Orientasi bersama Sari

**Pemicu dan prasyarat:** Mulai baru selesai prolog; Aji tiba di kelas dan bertemu Sari.

**Tindakan pemain:** Belajar berjalan, memutar kamera dan berinteraksi; mengikuti Sari ke empat area inti serta mengenal peran Pak Beni, Bu Ratih dan Pak Arif.

**Kondisi selesai:** Empat titik orientasi tercatat dan pemain menyelesaikan satu interaksi mandiri.

**Kriteria penerimaan:** Pemain yang tertinggal dapat menemukan Sari kembali; petunjuk menampilkan tombol yang benar; tur tidak mengunci akses menu jeda.

### M02 Mengamati tiga kejadian janggal

**Pemicu dan prasyarat:** M01 selesai; tiga adegan tersedia pada urutan cerita hari pertama.

**Tindakan pemain:** Mengamati Dion mengejek Bima, mengarahkan penolakan Laras, dan menerima buku tugas Raka yang tampak tertekan. Pemain memilih tidak ikut menertawakan atau mendukung.

**Kondisi selesai:** Ketiga observasi tersimpan sebagai indikasi awal dengan satu pertanyaan konteks pada masing-masing kasus.

**Kriteria penerimaan:** Jurnal tidak menyebut pola berulang sudah terbukti dari satu adegan; salah pilih mendapat penjelasan dan kesempatan mengulang tanpa memperburuk kejadian.

### M03 Uluran tangan pertama

**Pemicu dan prasyarat:** M02 selesai; jam istirahat memberi kesempatan mendekati ketiga teman.

**Tindakan pemain:** Menyapa Bima tanpa komentar tubuh, menawarkan menemani Laras, dan meminta izin membantu alat tulis Raka yang jatuh.

**Kondisi selesai:** Pemain memberi satu dukungan yang menghormati pilihan pada setiap teman; ringkasan chapter terbuka.

**Kriteria penerimaan:** Korban boleh menolak bicara atau ditemani. Respons memberi ruang tetap menyelesaikan subtujuan; tidak ada syarat pengakuan lengkap atau kepercayaan maksimum.

### Keluaran chapter

Aji dan Sari memahami bahwa ada situasi yang perlu diperhatikan lebih lanjut. Bima, Laras, dan Raka mengenal mereka sebagai teman yang dapat memberi dukungan; rincian pengulangan dan dampak dibuka pada chapter berikutnya. Checkpoint: awal Chapter 2.

## 8 Chapter 2 Di Balik Tawa di Lapangan

Tujuan: memahami ejekan tubuh, mendengar pengalaman Bima, dan memilih bantuan Pak Beni. Lokasi: lapangan dan tepi lapangan. Target durasi usulan 10–15 menit.

### M04 Mendengar cerita Bima

**Pemicu dan prasyarat:** M03 selesai; pada PJOK Dion kembali mengejek tubuh Bima dan Tio menertawakan.

**Tindakan pemain:** Menghampiri Bima di tepi lapangan, menanyakan kesediaan berbicara, dan mendengar bahwa tawa canggung dipakai agar ejekan cepat selesai.

**Kondisi selesai:** Pemain menyimak konteks pengulangan dan memilih respons yang mengakui ketidaknyamanan Bima.

**Kriteria penerimaan:** Percakapan berlangsung tanpa kerumunan; Sari hanya ikut bila Bima bersedia. Permintaan waktu dapat direspons dengan memberi ruang dan kembali pada titik aman.

### M05 Memilih bantuan yang aman

**Pemicu dan prasyarat:** Cerita Bima diketahui; ia ragu mengikuti pembagian tim berikutnya.

**Tindakan pemain:** Memilih langkah dari beberapa respons, lalu menawarkan pendampingan ke Pak Beni tanpa membalas hinaan atau mengajak berkelahi.

**Kondisi selesai:** Pilihan bantuan aman disepakati; jika Bima belum siap ditemani, Aji dapat berkonsultasi secara terbatas kepada guru tentang langkah perlindungan.

**Kriteria penerimaan:** Tidak ada cabang yang memaksa Bima langsung membuka cerita di depan Dion. Umpan balik menjelaskan alasan keselamatan, bukan menilai korban terlalu lemah.

### M06 Melibatkan Pak Beni

**Pemicu dan prasyarat:** M05 selesai; Pak Beni dapat ditemui dekat lapangan.

**Tindakan pemain:** Menyampaikan kejadian yang diketahui, memisahkan observasi dan cerita Bima, lalu melihat guru menghentikan komentar tubuh serta mengatur tim secara adil.

**Kondisi selesai:** Pak Beni menyatakan langkah penanganan dan pemantauan; Bima mendapat pilihan untuk ikut kegiatan kembali.

**Kriteria penerimaan:** Bima tidak harus menang olahraga atau mengubah berat badan untuk memperoleh bantuan. Pilihan istirahat sementara tetap menghasilkan akhir misi yang sah.

### Keluaran chapter

Komentar tubuh dihentikan pada situasi tersebut dan akses Bima pada kegiatan dipulihkan. Catatan untuk guru BK disimpan sebagai koordinasi guru, bukan tugas Aji mengungkap informasi pribadi. Kondisi Bima diperiksa lagi pada M14.

## 9 Chapter 3 Bangku yang Kosong

Tujuan: memahami pengucilan terarah dan menunjukkan bahwa satu ajakan berteman perlu disertai penghentian pola pengucilan oleh guru. Lokasi utama: kelas dan koridor dekat kelas. Target durasi usulan 10–15 menit.

### M07 Mendengar cerita Laras

**Pemicu dan prasyarat:** M06 selesai; tugas kelompok Bu Ratih memunculkan penolakan lagi atas ajakan Dion.

**Tindakan pemain:** Mendekati Laras secara tenang dan mendengar bahwa penolakan terjadi berulang, termasuk ajakan kepada siswa lain untuk menjauhinya.

**Kondisi selesai:** Pemain membedakan kebutuhan ruang pribadi yang wajar dari pengucilan terarah dalam konteks cerita.

**Kriteria penerimaan:** Tidak semua siswa digambarkan sama-sama menjadi pelaku. Ringkasan mencatat peran hasutan dan pola penolakan, tanpa menyalahkan sifat Laras.

### M08 Menawarkan tempat tanpa memaksa

**Pemicu dan prasyarat:** M07 selesai; Laras belum mendapat kelompok dan ragu bergabung.

**Tindakan pemain:** Menawarkan tempat bersama Aji dan Sari, menanyakan kebutuhan atau peran yang nyaman, dan menerima pilihan Laras.

**Kondisi selesai:** Laras bergabung secara sukarela atau memilih alternatif bantuan guru dan tempat belajar aman; pekerjaan kelompok tetap dapat berjalan.

**Kriteria penerimaan:** Penolakan ajakan bukan kegagalan misi. Guru dapat membantu alternatif kelompok tanpa memaksa Laras berteman dengan Dion atau menceritakan pengalaman di depan kelas.

### M09 Menyampaikan pola pengucilan

**Pemicu dan prasyarat:** M08 selesai; ada kebutuhan menghentikan hasutan di kelas meskipun kelompok sementara sudah tersedia.

**Tindakan pemain:** Menawarkan pendampingan untuk menyampaikan pola kejadian kepada Bu Ratih; jika Laras memilih bicara sendiri, Aji memberi ruang.

**Kondisi selesai:** Bu Ratih memeriksa informasi secara terpisah, menghentikan hasutan, mengatur kelompok yang adil, dan menyatakan tindak lanjut.

**Kriteria penerimaan:** Tindakan guru ditampilkan jelas. Misi tidak selesai hanya karena satu undangan kelompok; tidak ada sesi mempermalukan pelaku atau korban di depan kelas.

### Keluaran chapter

Laras memperoleh akses kelompok belajar yang aman dan mengetahui kepada siapa ia dapat meminta bantuan. Guru memantau pola pertemanan dan kerja kelompok. Penyelesaian tidak menuntut semua siswa menjadi sahabat Laras; follow-up dilakukan pada M14.

## 10 Chapter 4 Tekanan di Balik Tugas

Tujuan: mengenali pemaksaan tugas dan penyembunyian barang, memulihkan tas melalui bantuan guru, serta menyampaikan laporan ke BK dengan aman. Lokasi: kelas, koridor, ruang guru dan BK. Target durasi usulan 15–20 menit.

### M10 Mendengar pengakuan Raka

**Pemicu dan prasyarat:** M09 selesai; tas Raka hilang lagi setelah ia mencoba menolak menyerahkan jawaban PR.

**Tindakan pemain:** Mendengar cerita Raka dan keterangan Sari yang benar-benar ia ketahui. Jurnal mencatat waktu perkiraan, lokasi, kejadian, dan saksi tanpa menambah kesimpulan palsu.

**Kondisi selesai:** Kebutuhan utama ditentukan: bantuan mencari tas dan perlindungan dari tekanan tugas.

**Kriteria penerimaan:** Catatan boleh belum lengkap. Pemain tidak harus memata-matai Dion, mengambil foto, atau membuktikan semua ancaman sebelum bisa mendatangi guru.

### M11 Memulihkan barang dengan guru

**Pemicu dan prasyarat:** M10 selesai; guru tersedia untuk dimintai bantuan dan tas berada pada lokasi yang telah ditentukan skenario.

**Tindakan pemain:** Meminta bantuan guru, mengikuti pencarian di area yang diizinkan, dan membiarkan guru menangani pengembalian tas dari Fajar atau tempat penyimpanan.

**Kondisi selesai:** Tas kembali kepada Raka; guru membahas tekanan PR dan memastikan penilaian tugas ditinjau secara adil.

**Kriteria penerimaan:** Tidak ada pembobolan lemari, perampasan atau perkelahian. Tas tidak dapat digandakan setelah load atau interaksi berulang; temuan tas tidak menutup kebutuhan pelaporan.

### M12 Melapor ke ruang BK

**Pemicu dan prasyarat:** Tas sudah kembali; kekhawatiran balasan dan rangkaian kasus perlu ditindaklanjuti.

**Tindakan pemain:** Memilih rute biasa, rute tenang, atau pendamping guru. Pak Arif menerima keterangan secara privat dan terpisah sesuai kesediaan setiap siswa.

**Kondisi selesai:** Pak Arif menyatakan rencana perlindungan, koordinasi dengan guru terkait, dan waktu pemeriksaan kembali.

**Kriteria penerimaan:** Bertemu Tio atau Fajar tidak menyebabkan game over maupun pembatalan laporan. Jalur bantuan tetap tersedia walaupun jurnal belum lengkap; korban tidak dipaksa memberi kesaksian bersama.

### Keluaran chapter

Raka kembali memegang tas serta tugasnya sendiri. Sekolah menghentikan tekanan dan menyiapkan langkah menghadapi intimidasi lanjutan. Data kasus Bima dan Laras dapat dikoordinasikan antarguru; Aji tidak menjadi satu-satunya penghubung penanganan.

## 11 Chapter 5 Pemulihan dan Perubahan

Tujuan: memperlihatkan pertanggungjawaban pelaku, batas pengaruh latar keluarga, serta ukuran pemulihan yang berpusat pada rasa aman. Waktu cerita melompat beberapa hari. Target durasi usulan 10–15 menit.

### M13 Tindak lanjut perilaku pelaku

**Pemicu dan prasyarat:** M12 selesai; Pak Arif menindaklanjuti Dion, Tio, dan Fajar.

**Tindakan pemain:** Menyaksikan cutscene terpisah tentang komunikasi merendahkan dan kurangnya perhatian yang dialami Dion, serta bagaimana tawa Tio dan kepatuhan Fajar memperkuat perilakunya.

**Kondisi selesai:** Pemain melihat batas tegas sekolah: perilaku dihentikan, kerugian dipulihkan, konsekuensi edukatif ditetapkan, dan keluarga diajak mendukung perubahan.

**Kriteria penerimaan:** Aji tidak hadir atau menguping konseling privat. Adegan tidak menyebut keluarga sebagai penyebab tunggal; bantuan kepada Dion tidak mengurangi hak perlindungan korban.

### M14 Mengecek keadaan teman

**Pemicu dan prasyarat:** Tindak lanjut awal dilakukan; beberapa hari kemudian titik follow-up tersedia di lapangan, kelas dan koridor.

**Tindakan pemain:** Aji dan Sari menanyakan keadaan Bima, Laras, dan Raka dengan izin. Pak Arif menjalankan pemeriksaan privat; pemain mengenali jika masih ada kebutuhan bantuan.

**Kondisi selesai:** Tiga pemeriksaan tercatat dan kebutuhan yang belum selesai diteruskan kepada guru melalui pilihan dialog.

**Kriteria penerimaan:** Jawaban belum nyaman tidak dinilai sebagai kegagalan korban. Game menampilkan tindak lanjut tambahan, bukan memaksa jawaban semuanya baik-baik saja agar misi selesai.

### M15 Refleksi dan epilog

**Pemicu dan prasyarat:** M14 selesai; seluruh kebutuhan follow-up sudah memiliki langkah penanganan.

**Tindakan pemain:** Menjawab enam butir refleksi situasional tentang mengenali indikasi, dukungan, pelaporan, latar keluarga, pemulihan dan tindak lanjut.

**Kondisi selesai:** Enam butir dijawab dan umpan balik dibaca; epilog menampilkan tindakan nyata yang sedang dijalankan sekolah.

**Kriteria penerimaan:** Nilai rendah tidak mengunci epilog. Jawaban pertama dan perbaikan dibedakan untuk evaluasi; permintaan maaf atau memaafkan tidak dijadikan syarat kemenangan.

### Keluaran akhir

Pemain memahami bahwa dukungan teman membantu membuka akses pertolongan, sementara sekolah bertanggung jawab menghentikan tindakan serta memantau rasa aman. Penutup memperlihatkan perubahan perilaku yang mulai terjadi tanpa menjanjikan pelaku pasti berubah selamanya.

## 12 Kebutuhan fungsional dasar

Setiap kebutuhan Must harus memiliki hasil uji. Kriteria berikut berlaku di seluruh chapter, bukan hanya saat tutorial. ID FR digunakan pada matriks pengujian dan backlog.

| ID | Kebutuhan dan prioritas | Kriteria penerimaan |
| --- | --- | --- |
| FR01 | Must — gerak karakter | Aji dapat berjalan dan berlari ringan; berhenti pada collider; tidak menembus meja, dinding atau batas area pada jalur uji. |
| FR02 | Must — kamera third-person | Kamera mengikuti Aji, bisa diputar, memiliki batas pitch dan menghindari dinding; sensitivitas dapat diubah. Tidak ada split-screen. |
| FR03 | Must — interaksi kontekstual | Hanya satu target aktif dipilih dengan jelas; prompt menampilkan nama tindakan; objek di balik dinding tidak bisa diaktifkan. |
| FR04 | Must — tujuan dan status misi | Satu tujuan utama tampil; subtujuan diperbarui setelah event sah; urutan misi sesuai M01–M15; event berulang tidak menggandakan progres. |
| FR05 | Must — NPC dan pendamping | Sari dapat mengikuti atau berpindah ke titik aman. NPC tidak menghalangi pintu; jika tertinggal, ia pulih tanpa reset chapter. |
| FR06 | Must — peta dan penanda area | Nama ruang inti, posisi Aji, dan tujuan relevan tersedia; peta bisa dibuka dari gameplay serta ditutup tanpa kehilangan kontrol. |
| FR07 | Must — jurnal berbasis fakta | Catatan membedakan observasi, keterangan dan hal belum diketahui; tidak mewajibkan entri lengkap untuk mengakses guru. |
| FR08 | Must — menu dan jeda | Mulai baru, lanjutkan, pengaturan, bantuan dan keluar tersedia. Pause menghentikan pergerakan serta pemicu misi; mulai baru meminta konfirmasi karena mereset progres. |
| FR09 | Must — checkpoint dan continue | Progres tersimpan pada titik stabil; lanjutkan memulihkan misi, tujuan, flag dan posisi aman sesuai checkpoint. |
| FR10 | Should — bermain ulang | Chapter selesai dapat dipilih dari profil utama; mode ulang menggunakan sesi terpisah dan tidak merusak progres utama. |

### Kontrol default yang diusulkan

WASD untuk bergerak, mouse untuk kamera, Shift untuk lari ringan, E untuk interaksi, J untuk jurnal, M untuk peta, Esc untuk jeda atau kembali, Enter atau klik untuk melanjutkan dialog. Pilihan dialog dapat diakses dengan tombol arah dan Enter. Petunjuk harus mengikuti pemetaan aktual; tombol yang sama tidak boleh memicu dua aksi pada satu konteks.

## 13 Dialog dan perlindungan dalam gameplay

| ID | Kebutuhan dan prioritas | Kriteria penerimaan |
| --- | --- | --- |
| FR11 | Must — dialog bercabang lokal | Setiap pilihan punya respons dan tujuan lanjutan yang sah. Cabang dapat kembali ke alur utama tanpa menghapus pilihan yang relevan. |
| FR12 | Must — izin dan alternatif dukungan | Permintaan waktu, penolakan ajakan dan pilihan bicara sendiri memiliki respons yang menghormati korban serta jalur penyelesaian misi. |
| FR13 | Must — bantuan guru dapat diakses | Opsi meminta bantuan tidak terkunci oleh jumlah bukti, nilai refleksi atau tingkat kedekatan. Bantuan awal tidak otomatis menandai seluruh kasus selesai. |
| FR14 | Must — laporan privat | Keterangan korban tidak dipaksa tampil bersama atau diumumkan di kelas; koordinasi antarguru memiliki penjelasan di naskah. |
| FR15 | Must — rute tenang M12 | Rute tenang bersifat pilihan; bertemu pelaku tidak memicu kalah, hukuman, kehilangan jurnal atau batal menerima bantuan. |
| FR16 | Must — respons kurang aman | Pilihan membalas hinaan, memaksa atau merebut barang diberi alasan singkat dan kesempatan memperbaiki; tidak menghasilkan hadiah. |
| FR17 | Must — cutscene terkontrol | Adegan dapat dijeda dan teks dapat dilanjutkan secara manual. Jika dilewati, ringkasan pesan inti muncul sebelum pemain melanjutkan. |
| FR18 | Must — refleksi pembelajaran | Enam butir M15 tersedia dengan penjelasan tiap opsi. Jawaban pertama, jawaban perbaikan dan penyelesaian tersimpan terpisah. |
| FR19 | Must — follow-up ketiga kasus | M14 memeriksa ketiga korban; kebutuhan bantuan tambahan memiliki respons guru yang konkret dan tidak mengunci ending. |
| FR20 | Must — pengaturan dan bantuan | Volume, ukuran teks, sensitivitas kamera, pembalikan sumbu vertikal dan bantuan kontrol dapat dibuka sebelum serta selama bermain. |

### Pengecualian terhadap urutan misi

Alur utama mengikuti prasyarat chapter, tetapi permintaan bantuan keselamatan dapat menginterupsi tujuan aktif. Guru memberi respons kontekstual, menawarkan pendampingan atau tindakan awal, lalu game kembali ke titik cerita yang aman. Sistem tidak boleh mengatakan bantuan baru tersedia setelah semua tugas observasi selesai.

### Batas representasi sekolah

Tindakan guru yang ditampilkan adalah skenario edukasi, bukan rincian prosedur hukum atau kebijakan sekolah tertentu. Sebelum rilis, guru BK meninjau bahasa laporan, privasi, konsekuensi dan penanganan balasan agar sesuai konteks pemakaian.

## 14 Antarmuka dan aksesibilitas

| Layar atau elemen | Informasi dan tindakan wajib |
| --- | --- |
| Menu utama | Judul, mulai baru, lanjutkan jika save valid, pengaturan, bantuan dan keluar. |
| HUD gameplay | Tujuan aktif singkat, prompt interaksi dan petunjuk area bila diperlukan; jurnal dan peta tidak menutupi permainan terus-menerus. |
| Dialog | Nama pembicara, teks, opsi respons, indikator lanjut dan akses jeda; teks tidak berjalan otomatis secara wajib. |
| Jurnal dan peta | Ringkasan per kasus dan tujuan; status fakta; nama ruang. Catatan pribadi konseling tidak ditampilkan. |
| Ringkasan chapter | Apa yang dipahami, bantuan yang dilakukan dan hal yang masih ditindaklanjuti; tombol lanjut atau berhenti pada checkpoint. |
| Refleksi dan epilog | Soal situasional, umpan balik penjelasan, hasil belajar untuk pemain; lanjut tetap tersedia setelah semua soal dijawab. |
| Pengaturan | Audio, teks, kamera dan bantuan kontrol; perubahan berlaku tanpa menghapus progres. |
| Pesan kesalahan | Bahasa sederhana, tindakan pemulihan, dan akses kembali ke menu; tanpa kode teknis sebagai satu-satunya penjelasan. |

### Persyaratan aksesibilitas Must

- Semua dialog tersedia sebagai teks bahasa Indonesia. Jika ada suara, subtitle memuat nama pembicara dan tidak boleh memotong informasi penting.

- Ukuran teks dapat dinaikkan. Usulan uji awal: teks utama setara minimal 24 px pada 1920 × 1080, dengan dua opsi lebih besar; layout diuji kembali pada 1280 × 720.

- Pilihan, tujuan dan kesalahan dibedakan dengan teks atau ikon selain warna. Teks dan tombol diuji pada latar terang maupun gelap.

- Dialog dan menu utama dapat dioperasikan dengan keyboard. Fokus pilihan terlihat jelas; tombol lanjut tidak sekaligus memilih respons berikutnya.

- Tidak ada batas waktu pada pilihan bantuan. Pemain dapat membaca ulang instruksi, menjeda, dan keluar ke checkpoint tanpa penilaian negatif.

- Camera shake dan motion blur tidak digunakan pada versi inti. Kecepatan kamera dapat diturunkan; perpindahan adegan tidak memakai kilatan cepat.

Aksesibilitas diperiksa pada prototipe bersama pengguna sasaran. Dukungan pembaca layar penuh, controller dan remapping lanjutan belum menjadi komitmen Must; keterbatasan aktual ditulis pada catatan rilis, bukan diasumsikan telah teratasi.

## 15 Arah visual audio dan daftar aset

Gaya smooth low-poly menggunakan bentuk sederhana, shading lembut dan detail secukupnya agar emosi serta interaksi terbaca. Rasio badan terhadap kepala 2:1 berarti tinggi badan di bawah kepala sekitar dua kali tinggi kepala, sehingga tinggi keseluruhan sekitar tiga kepala. Proporsi dinilai dari model berdiri netral.

| Kelompok | Isi minimum | Catatan produksi |
| --- | --- | --- |
| Karakter siswa | 8 karakter inti: Aji, Sari, Bima, Laras, Raka, Dion, Tio, Fajar. | Seragam SMP putih dan biru tua; emblem OSIS kuning pada saku. Variasi siluet dan rambut jelas. |
| Karakter dewasa | Pak Beni, Bu Ratih, Pak Arif, Ayah Aji, Ibu Aji. | Gunakan rig dan material bersama bila cocok; dua orang tua cukup hadir dalam prolog. |
| Identitas rambut | Aji rapi agak ikal tanpa low fade; Raka belah tengah dan berkacamata; Dion mullet. | Gaya rambut adalah identitas visual, bukan penjelasan bahwa suatu potongan berarti anak pasti nakal. |
| Animasi | Idle, berjalan, lari ringan, berputar, duduk, bicara, menunjuk, memberi/menerima, mengambil alat tulis. | Variasi emosi ringan melalui pose, ekspresi sederhana, dan dialog; rig diuji sebelum seluruh karakter dibuat. |
| Kelas | Meja dan kursi siswa, meja guru, papan tulis, lemari, buku, tas, alat tulis, pintu, jendela. | Prop tas Raka dan alat tulis harus memiliki state cerita dan collider yang sesuai. |
| Sekolah | Lapangan, garis lapangan, bola, bangku tepi, koridor, tanda ruang, furnitur ruang guru dan BK. | Modular; ukuran diuji melalui blockout dan kamera. Satu set kelas cukup untuk alur inti. |
| UI dan audio | Ikon interaksi, jurnal, peta, tujuan; langkah kaki, bel, ambience sekolah, musik tenang. | Voice-over lengkap opsional; seluruh informasi tetap dapat dipahami tanpa suara. |

### Kriteria kesiapan aset

Setiap aset memiliki nama, asal/lisensi, status rig atau collider, material, dan lokasi pemakaian. Aset dari alat generatif tetap harus diperiksa lisensi penggunaan, topologi, skala, clipping seragam, dan kompatibilitas animasi. Jumlah polygon dan ukuran tekstur dikunci setelah pengukuran pada perangkat uji; tidak diputuskan dari tampilan preview saja.

## 16 Struktur proyek Unity arsitektur dan penyimpanan

### 16 1 Struktur folder yang menjadi acuan

Root proyek game adalah `Assets/_Game/`. Nama dan kapitalisasi folder yang terlihat pada panel Project dipertahankan, termasuk `assetEnv`, `texture`, `Character Setup`, dan `env_design`. Tangkapan layar memperlihatkan folder; keberadaan file scene, skrip atau prefab di dalam folder yang tidak dibuka belum dapat dipastikan. Nama file serta subfolder tambahan dalam rancangan di bawah merupakan usulan implementasi.

| Path relatif terhadap `Assets/_Game/` | Peran dalam proyek | Aturan penempatan |
| --- | --- | --- |
| `Art/` | Sumber visual karakter, animasi dan grafis UI. | Gunakan untuk sumber model karakter dan visual selain environment; prefab siap pakai tetap di `Prefabs/`. |
| `assetEnv/` | Sumber model environment yang diimpor. | Aset bangunan, lapangan, gawang, furnitur dan prop sekolah; prefab hasil perakitannya ditempatkan di `Prefabs/`. |
| `Audio/` | Musik, ambience, efek suara dan voice-over jika dibuat. | Kelompokkan menurut fungsi jika diperlukan; gameplay tetap terbaca tanpa suara. |
| `Data/` | Data chapter, misi, dialog, refleksi dan pemetaan scene. | Data desain yang dibaca game; bukan tempat menulis save pemain pada build. |
| `Materials/` | Material bersama dan katalog material proyek. | Material baru yang dikelola proyek boleh memakai awalan `MAT_`; hindari salinan per chapter tanpa kebutuhan visual. |
| `Materials/Materials/` | Material yang sudah tersimpan pada struktur saat ini. | Pertahankan referensi yang dipakai model atau prefab; pemeriksaan duplikasi dilakukan sebelum pemindahan. |
| `Materials/texture/` | Tekstur environment dan material terkait. | Pertahankan penamaan folder yang ada; tekstur dipakai melalui material bersama. |
| `Materials/texture/Materials/` | Subfolder material pada direktori tekstur yang sudah ada. | Inventarisasi pemakaian sebelum konsolidasi; tidak diwajibkan dihapus atau diganti nama. |
| `Prefabs/` | Objek siap pakai yang menyatukan model, material, collider dan komponen. | Menyimpan karakter, lingkungan sekolah, prop interaktif dan UI; subfolder tambahan bersifat usulan. |
| `Resources/` | Direktori yang sudah tersedia untuk aset yang memang memerlukan pemuatan dari sana. | Tidak wajib menyalin seluruh aset ke sini; data misi dan visual biasa dapat memakai referensi langsung. |
| `Scenes/` | Folder seluruh scene proyek. | Pisahkan inisialisasi, menu, setup karakter, level dan pengujian sesuai struktur yang ada. |
| `Scripts/` | Kode sistem permainan. | Modul dipisahkan menurut tanggung jawab, tanpa mewajibkan folder baru pada root `Assets/`. |

Folder `Materials/Materials/` dan `Materials/texture/Materials/` dicatat sebagai bagian struktur yang ada. Revisi PRD ini mengatur penggunaannya tanpa memerintahkan perombakan otomatis terhadap aset yang sudah terhubung.

### 16 2 Scene dan pemetaan chapter

| Folder yang sudah terlihat | File scene usulan | Fungsi dan isi |
| --- | --- | --- |
| `Scenes/00_Bootstrap/` | `Bootstrap.unity` | Titik masuk build; menyiapkan layanan progres, pengaturan, transisi dan save sebelum membuka menu. |
| `Scenes/01_MainMenu/` | `MainMenu.unity` | Mulai baru, lanjutkan, pengaturan, bantuan dan keluar. |
| `Scenes/Character Setup/` | `CharacterSetup.unity` | Pengujian proporsi 2:1, rig, animasi, seragam dan material karakter; bukan alur cerita pemain. |
| `Scenes/Levels/env_design/` | `School_Blockout.unity` | Ruang kerja untuk tata sekolah, skala, jalur, pencahayaan dan pengujian kamera sebelum dipakai pada level. |
| `Scenes/Levels/Level_01/` | `Level_01.unity` | Chapter 1, M01–M03; orientasi, observasi dan dukungan awal. |
| `Scenes/Levels/Level_02/` | `Level_02.unity` | Chapter 2, M04–M06; kasus Bima di lapangan. |
| `Scenes/Levels/Level_03/` | `Level_03.unity` | Chapter 3, M07–M09; pengucilan Laras dan kelompok belajar. |
| `Scenes/Levels/Level_04/` | `Level_04.unity` | Chapter 4, M10–M12; tas Raka, pemaksaan tugas dan akses BK. |
| `Scenes/Levels/Level_05/` | `Level_05.unity` | Chapter 5, M13–M15; tindak lanjut, follow-up dan epilog. |
| `Scenes/Test/` | `Test_Interaction.unity`, `Test_SaveLoad.unity` dan scene uji sesuai kebutuhan | Menguji sistem secara terpisah sebelum integrasi; tidak ditawarkan pada menu pemain. |

Pemetaan ini tidak mengharuskan file scene yang sudah ada diganti nama. Saat inventarisasi proyek, catat nama file aktual pada daftar scene. Jika nama aktual berbeda, perbarui referensi data dan daftar build; folder `Level_01`–`Level_05` serta pemetaan chapter tetap dipertahankan.

### 16 3 Pemakaian ulang environment

Rancangan awal memakai satu scene gameplay aktif untuk setiap chapter. Kelima level menggunakan sumber environment sekolah yang sama melalui prefab bersama, misalnya `Assets/_Game/Prefabs/Environment/SchoolEnvironment.prefab`. Path prefab ini adalah usulan; subfolder `Environment/` boleh dibuat di dalam `Prefabs/`.

- `env_design` menjadi tempat merancang dan memeriksa sekolah. Geometri yang siap dipakai diterapkan pada sumber prefab lingkungan bersama.
- `Level_01`–`Level_05` memuat instance prefab sekolah, karakter, kamera dan pengaturan kejadian khusus chapter. Sekolah tidak dibuat ulang menjadi lima sumber model yang terpisah.
- Detail per chapter meliputi posisi NPC, buku atau tas, pemicu dialog, akses ruang, waktu cerita dan checkpoint. Prop yang memiliki state cerita dipulihkan dari progres, bukan dibiarkan mengikuti kondisi awal scene.
- Lokasi kelas, lapangan, koridor dan BK tetap konsisten antarlevel. Perubahan sumber prefab diperiksa di kelima level sebelum rilis.
- Hanya satu Aji, satu kamera gameplay, satu HUD dan satu pengendali misi yang aktif. Hindari menduplikasi objek ini di dalam prefab environment dan root level sekaligus.

Pemuatan environment secara additive dapat dipertimbangkan setelah profiling jika dibutuhkan. Baseline implementasi menggunakan pergantian scene level dengan prefab lingkungan bersama; hasil pengukuran diperlukan sebelum menambah kompleksitas pemuatan.

### 16 4 Alur inisialisasi dan transisi

1. Build dimulai dari scene pada `Scenes/00_Bootstrap/`. Layanan lintas scene dibuat satu kali, lalu menu utama dibuka.
2. **Mulai baru** menginisialisasi progres Chapter 1, menjalankan prolog singkat, lalu memulai M01 pada `Level_01`. Prolog dapat berada pada rangkaian awal level tersebut sehingga tidak memerlukan folder scene tambahan.
3. **Lanjutkan** memvalidasi save, membaca `chapterId` dan `sceneId`, kemudian membuka level terkait pada `checkpointId` yang aman.
4. Setelah tiga misi chapter selesai, game menampilkan ringkasan, menyimpan checkpoint transisi, lalu membuka level berikutnya. Perpindahan normal adalah `Level_01` sampai `Level_05`; transisi level lama dibersihkan sebelum interaksi level baru diaktifkan.
5. Jika pemuatan atau penyimpanan gagal, pemain menerima pilihan mencoba kembali atau kembali ke menu. Progres terakhir yang valid tidak langsung ditimpa.
6. Menyelesaikan M15 membuka epilog dan akses menu. Bermain ulang, jika fitur Should ini dibuat, memakai sesi terpisah dari progres utama.

Scene rilis yang diperlukan ialah Bootstrap, MainMenu dan kelima level gameplay. `Character Setup`, `env_design` dan `Test` adalah area kerja pengembangan dan tidak dimasukkan sebagai tujuan pemain pada build rilis. Membuka level langsung saat pengembangan harus tetap memakai inisialisasi yang terkontrol, tanpa membuat layanan ganda.

### 16 5 Penempatan modul skrip

Semua kode game ditempatkan di `Assets/_Game/Scripts/`. Subfolder berikut merupakan usulan pengelompokan di dalam folder yang sudah ada, bukan klaim bahwa file-file tersebut telah dibuat.

| Subfolder usulan | Tanggung jawab | Batas sistem |
| --- | --- | --- |
| `Core/` | Inisialisasi sesi, status permainan dan perpindahan scene. | Layanan dibuat satu kali; transisi tidak menggandakan player atau manager. |
| `Player/` dan `Camera/` | Gerak, collision, input dan kamera third-person. | Tidak menentukan sendiri keputusan cerita atau status penanganan guru. |
| `Interaction/` | Pemilihan target dan pemicu interaksi. | Memeriksa jarak, konteks, dan akses; tidak menembus dinding. |
| `NPC/` | Posisi, interaksi, pendampingan dan pemulihan NPC. | Sari atau NPC tidak boleh mengunci pintu dan alur misi. |
| `Missions/` | Prasyarat, status, subtujuan dan penerimaan event. | Event berulang tidak menggandakan hasil; data chapter mengatur misi aktif. |
| `Dialogue/` | Node dialog, pilihan, izin dukungan dan flag naratif. | Memisahkan pengetahuan pemain dari pengetahuan Aji, terutama pada M13. |
| `UI/` | Menu, HUD, jurnal, peta, pengaturan dan refleksi. | UI membaca progres; tidak menjadi sumber status misi kedua. |
| `SaveLoad/` | Save berversi, validasi, backup dan pemulihan checkpoint. | Save runtime berada pada lokasi data pengguna, bukan di `Assets/_Game/Data/`. |

### 16 6 Data desain dan identitas level

Data misi dan dialog dipisahkan dari kode dalam ScriptableObject atau format terstruktur yang dipilih pengembang. Subfolder `Data/Chapters/`, `Data/Missions/`, `Data/Dialogues/`, `Data/Reflections/` dan `Data/SceneRegistry/` adalah usulan. Setiap data memiliki ID unik dan referensi yang dapat divalidasi.

| ID chapter | ID scene logis | Misi | Data konfigurasi minimal |
| --- | --- | --- | --- |
| `CH01` | `Level_01` | `M01`–`M03` | Orientasi, tiga observasi, dukungan awal dan spawn hari pertama. |
| `CH02` | `Level_02` | `M04`–`M06` | Kejadian lapangan, percakapan Bima dan langkah Pak Beni. |
| `CH03` | `Level_03` | `M07`–`M09` | Kelompok kelas, pilihan Laras dan langkah Bu Ratih. |
| `CH04` | `Level_04` | `M10`–`M12` | State tas Raka, pencarian bersama guru, rute tenang dan laporan BK. |
| `CH05` | `Level_05` | `M13`–`M15` | Cutscene pelaku, tiga follow-up, enam butir refleksi dan epilog. |

Daftar scene memetakan ID logis ke file scene aktual. Save menyimpan ID yang stabil, bukan urutan numerik scene dalam daftar build atau path absolut komputer pengembang. Validator memeriksa referensi dialog, misi, prefab dan scene yang hilang. Nama folder dan file harus sesuai kapitalisasi yang dicatat.

### 16 7 Skema minimum save

Satu profil lokal menyimpan: `schemaVersion`, `buildVersion`, `chapterId`, `sceneId`, `missionId`, `objectiveStates`, `storyFlags`, `supportChoices`, `journalEntries`, `reflectionFirstAnswers`, `reflectionRetries`, `completedChapters`, `checkpointId`, dan `settings`. Nama siswa nyata, kontak, identitas sekolah atau pengalaman pribadi pemain tidak diminta.

Save pemain ditulis ke lokasi data pengguna yang dapat ditulis pada build Windows. `Data/` menyimpan konfigurasi desain dan `Resources/` tidak menjadi lokasi save runtime. Pengaturan dapat berada dalam file terpisah agar reset cerita tidak menghapus preferensi kamera atau teks.

### 16 8 Aturan simpan dan pulihkan

- Autosave pada akhir misi dan transisi aman. Jika pemain keluar di tengah adegan, beri keterangan bahwa permainan kembali ke checkpoint terakhir.
- Tulis file sementara, validasi, lalu ganti file utama; pertahankan satu backup sebelumnya. Kegagalan simpan memberi pesan dan opsi mencoba kembali.
- Saat pergantian chapter, checkpoint tujuan menjadi aktif setelah scene tujuan siap. Jika transisi terputus, continue menggunakan checkpoint terakhir yang valid.
- Load membuka scene berdasarkan registry, menempatkan Aji pada spawn aman, lalu memulihkan NPC, tas Raka, jurnal dan flag cerita sebelum input gameplay diaktifkan.
- Kamera dan trigger tidak meneruskan frame cutscene yang terputus. Satu event misi tidak boleh dijalankan ulang hanya karena scene baru selesai dimuat.
- Save rusak mencoba backup; bila keduanya tidak valid, jelaskan kondisi dan tawarkan mulai baru. Jangan menghapus progres lama tanpa tindakan pemain.
- Mode bermain ulang Should memakai data sesi sehingga penyelesaian ulang tidak menimpa jawaban pertama utama.

### 16 9 Kebutuhan penerimaan struktur proyek

| ID | Kebutuhan dan prioritas | Kriteria penerimaan |
| --- | --- | --- |
| FR21 | Must — kesesuaian struktur proyek | Root `Assets/_Game/` dan folder yang teramati dipertahankan; aset, prefab, data dan skrip memiliki lokasi serta referensi yang tercatat. Nama file baru ditandai dalam inventaris sampai diimplementasikan. |
| FR22 | Must — pemetaan level dan transisi | Chapter 1–5 dimuat melalui scene pada folder `Level_01`–`Level_05`; continue membuka chapter dan checkpoint yang tepat; player, kamera, HUD dan layanan tidak ganda setelah transisi. |

### 16 10 Konvensi aset dan pemeriksaan referensi

Nama file baru dapat memakai pola `PF_` untuk prefab, `MAT_` untuk material, `SFX_` untuk efek suara dan `SO_` untuk data ScriptableObject jika format itu dipilih. Konvensi ini berlaku pada aset baru; aset lama tidak wajib diganti nama serentak.

Sebelum memindahkan atau mengganti nama aset yang sudah dipakai, periksa referensi material, prefab, scene dan data. Inventaris mencatat path aktual, tipe, kegunaan, sumber atau lisensi, serta level pemakaian. Perubahan struktur dilakukan melalui pengelolaan aset Unity dan diikuti pemeriksaan referensi serta uji build.

## 17 Kebutuhan nonfungsional dan build

Target di bawah adalah usulan penerimaan teknis, bukan hasil benchmark. Pada vertical slice, tetapkan satu laptop uji utama dan catat CPU, GPU, RAM, OS serta resolusi. Spesifikasi minimum rilis hanya boleh diumumkan sesudah build diuji pada perangkat tersebut.

| ID | Persyaratan | Target dan verifikasi |
| --- | --- | --- |
| NFR01 | Performa | Usulan median ≥30 FPS pada 1080p preset rendah di laptop uji utama; ukur juga 1% low, puncak frame time dan lokasi stutter selama rute 10 menit. |
| NFR02 | Waktu tunggu | Usulan masuk gameplay ≤30 detik, transisi chapter ≤10 detik, dan simpan ≤1 detik pada perangkat uji; tampilkan indikator jika operasi memerlukan waktu. |
| NFR03 | Stabilitas | Satu sesi end-to-end 15 misi dan satu sesi continue lintas chapter tanpa crash, kehilangan progres atau soft lock. |
| NFR04 | Kompatibilitas | Build Windows 64-bit dengan keyboard dan mouse; uji 1280 × 720 serta 1920 × 1080. Versi OS yang didukung dicatat dari hasil uji aktual. |
| NFR05 | Operasi offline | Semua misi, teks, audio, pengaturan dan penyimpanan inti berjalan tanpa jaringan; tidak mewajibkan login atau layanan eksternal. |
| NFR06 | Privasi | Tidak ada formulir identitas anak, chat, perekaman suara atau telemetri jaringan pada versi inti; data lokal dapat dihapus melalui reset dengan konfirmasi. |
| NFR07 | Pemeliharaan | ID chapter, misi dan dialog unik; registry memetakan ID scene ke file aktual; validator menemukan referensi scene, prefab dan data yang hilang sesuai Bagian 16. |
| NFR08 | Keterbacaan | Tidak ada teks terpotong atau tombol tertutup pada dua resolusi uji dan seluruh ukuran teks; semua suara dialog memiliki padanan teks. |
| NFR09 | Distribusi | Build dimulai dari Bootstrap dan dapat dijalankan tanpa Unity Editor; scene setup, blockout dan uji tidak menjadi alur rilis. README memuat kontrol, cara lanjutkan, lokasi save, versi serta keterbatasan teruji. |

### Batas produksi yang perlu diukur

Profiling memeriksa kamera di ruang sempit, kerumunan NPC, material, pencahayaan dan pemuatan scene. Gunakan ulang aset, collider sederhana, pencahayaan yang sesuai dan jumlah NPC aktif terbatas berdasarkan hasil pengukuran. Ukuran build dan pemakaian memori dicatat pada setiap kandidat rilis; anggarannya ditetapkan sesudah vertical slice, sebelum produksi seluruh chapter.

## 18 Rencana pengujian fungsional

Uji gameplay dan transisi dijalankan pada build terpaket, bukan hanya Editor. Audit struktur TC16 memeriksa proyek sumber dan referensinya di Editor, kemudian dilengkapi uji build. Catat langkah reproduksi, hasil, versi build, perangkat dan bukti kegagalan. Semua TC01–TC14 serta TC16–TC17 wajib lulus sebelum uji siswa. TC15 hanya berlaku jika fitur bermain ulang dibuat.

| ID | Skenario dan acuan | Hasil yang diterima |
| --- | --- | --- |
| TC01 | Rute kelas–lapangan–BK; FR01–03 | Aji dan kamera melewati pintu serta meja tanpa tembus, macet atau interaksi di balik dinding. |
| TC02 | Seluruh 15 misi; FR04 | Prasyarat, subtujuan dan transisi benar; tidak ada misi yang hilang atau terulang tanpa sengaja. |
| TC03 | Sari tertinggal dan pintu dihalangi; FR05 | Pendamping pulih ke titik aman; pemain dapat melanjutkan tanpa reset chapter. |
| TC04 | Observasi pertama M02; FR07 | Jurnal mencatat indikasi dan hal belum diketahui, tidak mengklaim pola sudah terbukti. |
| TC05 | Korban menolak bicara/ajakan; FR11–12 | Pilihan memberi ruang dan alternatif bantuan tetap menyelesaikan tujuan yang sesuai. |
| TC06 | Lapor dengan catatan belum lengkap; FR13 | Guru memberi bantuan awal; tidak muncul tuntutan melengkapi bukti sebagai syarat perlindungan. |
| TC07 | Tio/Fajar terlihat pada M12; FR15 | Tidak game over, progres tetap ada dan laporan BK tetap dapat dilakukan. |
| TC08 | Pilihan hinaan, paksa, rebut; FR16 | Penjelasan muncul dan pemain dapat memperbaiki respons; tidak ada hadiah perilaku merugikan. |
| TC09 | Lewati/jeda cutscene M13; FR14,17 | Ringkasan pesan inti tersedia; Aji tidak memperoleh akses ke konseling privat. |
| TC10 | Jawaban salah pada refleksi; FR18 | Umpan balik dan percobaan ulang tersedia; jawaban pertama tidak tertimpa; ending tetap dapat dicapai. |
| TC11 | Keluar/load tiap akhir misi; FR09 | Misi, prop tas, jurnal dan tujuan pulih tepat; trigger tidak menggandakan barang atau progres. |
| TC12 | Save tidak valid dan write gagal; FR09 | Backup atau pesan pemulihan muncul; tidak diam-diam menghapus data. |
| TC13 | Teks besar dan keyboard; FR20, NFR08 | Dialog, fokus dan pilihan terbaca pada kedua resolusi; tidak ada input ganda. |
| TC14 | Putus jaringan dan PC tanpa Editor; NFR05,09 | Game inti berjalan offline dari paket rilis; README cocok dengan perilaku aktual. |
| TC16 | Audit struktur dan referensi; FR21, NFR07 | Folder sesuai Bagian 16; sumber environment, prefab, data dan skrip tercatat; tidak ada referensi material, scene atau data yang hilang. |
| TC17 | Bootstrap, lima level dan continue; FR22 | Mulai baru membuka Level_01; lanjut dari checkpoint mengarah ke level yang benar; transisi sampai Level_05 tidak menggandakan Aji, kamera, HUD atau layanan. |

TC15 bersyarat untuk fitur Should bermain ulang: jalankan chapter selesai lalu kembali ke continue; progres utama dan jawaban pertama harus tetap sama. Uji performa NFR01–02 memakai rute baku yang sama antarbuild.

## 19 Evaluasi isi usability dan pembelajaran

### Tahap 1 Penelaahan ahli

Sebelum uji siswa, pembimbing dan guru BK atau penelaah kompeten meninjau ketepatan bentuk perundungan, bahasa, respons saksi, peran guru, privasi, latar keluarga Dion dan ending. Gunakan lembar penilaian beserta komentar per adegan. Temuan yang menyalahkan korban, mensyaratkan bukti lengkap, atau memaksa pengungkapan harus diperbaiki dahulu.

### Tahap 2 Uji penggunaan

Minta peserta menjalankan tutorial, mencari guru, memilih dukungan, membuka jurnal, serta melanjutkan dari save. Amati waktu, kesalahan, permintaan bantuan, kebingungan kamera dan keterbacaan. Jelaskan bahwa yang diuji adalah produk. Jangan mengarahkan jawaban saat tugas berjalan; bantuan peneliti dicatat agar hasil tidak tampak lebih baik dari kondisi sebenarnya.

### Tahap 3 Evaluasi hasil belajar

| Bagian | Rancangan instrumen yang diusulkan |
| --- | --- |
| Pretest dan posttest | Masing-masing 6–12 soal situasional yang dipetakan ke LP01–LP06; gunakan bentuk setara, bahasa SMP, dan kunci yang ditinjau penelaah. |
| Refleksi dalam game | Enam butir M15 untuk umpan balik. Karena jawaban dapat diulang, gunakan jawaban pertama dan jangan menyamakannya dengan posttest independen. |
| Analisis | Laporkan jumlah peserta, skor awal/akhir, sebaran perubahan, keberhasilan tugas dan masalah penggunaan. Pemilihan analisis statistik mengikuti desain dan persetujuan pembimbing. |
| Interpretasi | Pretest–posttest satu kelompok dapat menunjukkan perubahan pada sampel, tetapi belum membuktikan bahwa game sendirian menyebabkan perubahan atau mengurangi perundungan nyata. |

### Pelaksanaan dengan peserta SMP

- Jumlah peserta, tempat, durasi dan mekanisme izin ditetapkan bersama pembimbing serta sekolah sebelum rekrutmen; ikuti persetujuan orang tua/wali dan kesediaan siswa sesuai ketentuan penelitian.

- Gunakan kode peserta. Pemain tidak diminta menuliskan nama pelaku/korban nyata atau pengalaman pribadi. Data penelitian terpisah dari save game.

- Peserta boleh menjeda atau berhenti. Jika muncul ketidaknyamanan atau pengungkapan kejadian nyata, fasilitator merespons secara privat dan menghubungkan dengan guru yang bertanggung jawab sesuai prosedur sekolah.

- Dokumentasikan perbaikan setelah pilot sebelum pengambilan data utama. Jangan menyebut hasil evaluasi positif sebelum data terkumpul dan dianalisis.

## 20 Tahap pengembangan dan prioritas backlog

Usulan jadwal adalah 10 minggu untuk pengembangan inti setelah desain disepakati, dengan asumsi pengembang utama tunggal dan aset modular. Ini perkiraan kerja; sesuaikan dengan kalender TA, kemampuan rigging dan waktu akses pengujian sekolah. Sisakan waktu perbaikan sebelum pengambilan data utama.

| Tahap | Waktu usulan | Keluaran dan syarat lanjut |
| --- | --- | --- |
| P01 Desain siap produksi | Minggu 1 | PRD, naskah 15 misi, alur data, review BK awal, daftar aset dan perangkat uji disepakati. |
| P02 Vertical slice | Minggu 2–3 | Satu potongan M04–M06 yang memuat gerak, kamera, dialog, respons salah, bantuan guru, jurnal dan save; ukur performa sebelum membuat seluruh aset. |
| P03 Sistem dan sekolah inti | Minggu 4 | Struktur Assets/_Game dan registry scene sesuai Bagian 16; blockout di env_design; prefab sekolah bersama; UI, misi, dialog dan checkpoint stabil. |
| P04 Produksi alur lengkap | Minggu 5–7 | Level_01–Level_05 memuat 15 misi dengan sumber sekolah bersama; seluruh cabang aman, cutscene, transisi dan ending dapat dimainkan. |
| P05 Alpha dan pilot | Minggu 8 | TC wajib lulus; validasi isi lanjutan dan pilot penggunaan; daftar perbaikan diprioritaskan. |
| P06 Kandidat rilis | Minggu 9–10 | Perbaikan, regression test, evaluasi yang disetujui pembimbing, paket build, README dan dokumentasi hasil. |

### Urutan pekerjaan yang disarankan

- Paling awal: pastikan satu karakter dapat bergerak dan berinteraksi, satu percakapan bercabang selesai, bantuan guru tersedia, lalu progres bisa disimpan dan dipulihkan.

- Berikutnya: bangun quest dan dialog berbasis data, sekolah modular, jurnal fakta, serta jalur alternatif saat korban menolak ajakan.

- Setelah alur lengkap: rapikan kamera, animasi, suara, keterbacaan dan transisi; lakukan pengujian siswa hanya setelah perbaikan isi kritis.

- Fitur Should/Could masuk setelah 15 misi stabil. Jika jadwal tertekan, kurangi variasi dekorasi, voice-over atau animasi, sambil mempertahankan pesan dan tindakan guru.

Ketergantungan utama: naskah harus ditinjau sebelum voice-over; rig harus diuji sebelum membuat semua karakter; tata ruang harus lolos kamera sebelum dekorasi; instrumen harus disetujui sebelum data penelitian dikumpulkan.

## 21 Risiko dan keputusan implementasi

| Risiko | Dampak | Mitigasi dan penanggung jawab |
| --- | --- | --- |
| Cakupan dialog dan aset membesar | Alur inti tidak selesai. | Pengembang membatasi cabang lokal dan sekolah kompak; pembimbing meninjau perubahan ruang lingkup. |
| Aset generatif sulit dirig atau tidak konsisten | Animasi rusak dan waktu revisi tinggi. | Pengembang menguji satu model lengkap terlebih dahulu, mencatat lisensi dan memakai rig bersama bila cocok. |
| Kamera tidak nyaman di kelas | Pemain tersesat atau mual. | Pengembang menguji blockout, collision kamera, kecepatan putar dan ukuran jalur sebelum detail aset. |
| Cerita menyalahkan korban atau keluarga | Pesan pendidikan keliru. | Penelaah BK memeriksa dialog dan cutscene; temuan kritis menahan rilis. |
| Save atau event misi tidak konsisten | Progres hilang atau soft lock. | Pengembang memakai ID unik, checkpoint stabil, backup, validator dan TC11–12. |
| Hasil belajar ditafsirkan berlebihan | Kesimpulan TA tidak sesuai data. | Peneliti dan pembimbing memisahkan refleksi, tes independen, usability dan klaim dampak nyata. |
| Uji sekolah tertunda | Jadwal evaluasi meleset. | Peneliti menyiapkan izin dan instrumen lebih awal; jadwal utama memuat cadangan waktu sesuai kalender TA. |

### Daftar keputusan yang perlu dikunci

| Keputusan | Default perencanaan | Waktu penetapan |
| --- | --- | --- |
| Versi Unity dan paket | Satu versi stabil yang tersedia pada perangkat pengembang; tidak berganti di tengah produksi tanpa alasan. | Sebelum P02. |
| Perangkat uji minimum | Belum diklaim; pilih laptop sekolah atau perangkat representatif dan ukur. | Pada P02. |
| Voice-over dan wajah | Teks lengkap dan ekspresi sederhana; suara penuh opsional. | Sebelum P04. |
| Durasi sesi dan jumlah peserta | Usulan total bermain 60–90 menit; jumlah dan rancangan sampel bersama pembimbing. | Sebelum P05. |
| Format distribusi | Folder build Windows offline dan README; pemasang opsional. | Sebelum P06. |

Perubahan setelah P03 dicatat dengan alasan, kebutuhan terdampak, tambahan pekerjaan dan keputusan pembimbing. Penambahan fitur tidak boleh menghilangkan perlindungan, alternatif respons atau follow-up yang sudah ditetapkan.

## 22 Ketertelusuran dan definisi selesai

| Hasil belajar | Misi utama | Kebutuhan dan uji |
| --- | --- | --- |
| LP01 Mengenali indikasi dan konteks | M02, M04, M07, M10 | FR04, FR07; TC02, TC04; soal pretest/posttest. |
| LP02 Memahami tawa atau diam | M04, M05 | FR11–12, FR16; TC05, TC08; refleksi Bima. |
| LP03 Dukungan tanpa memaksa | M03, M08, M14 | FR12, FR19; TC05; penelaahan dialog. |
| LP04 Bantuan aman dan peran guru | M05–06, M09, M11–12 | FR13–16; TC06–08; tugas usability mencari bantuan. |
| LP05 Pemulihan dan follow-up | M12, M14–15 | FR18–19; TC10; penelaahan ending. |
| LP06 Latar keluarga dan tanggung jawab | M13, M15 | FR14, FR17–18; TC09–10; soal tentang latar Dion. |

Ketertelusuran teknis tambahan: FR21 diuji melalui TC16 untuk struktur dan referensi aset; FR22 diuji melalui TC17 serta TC11 untuk pemetaan level, transisi dan pemulihan checkpoint.


### Satu misi dinyatakan selesai dikembangkan jika

- Pemicu, prasyarat, tujuan, dialog, tindakan guru dan kondisi selesai berjalan sesuai rancangan; semua cabang memiliki jalan kembali yang aman.

- Misi dapat dilanjutkan dari checkpoint dan tidak menggandakan event, jurnal, jawaban atau prop saat diulang.

- Teks, nama tokoh, lokasi dan petunjuk kontrol sesuai versi final; ada umpan balik untuk pilihan kurang aman.

- Kriteria penerimaan misi serta kebutuhan FR terkait telah diuji, dan naskah tidak memiliki temuan isi kritis.

### Versi akhir TA dinyatakan siap diserahkan jika

- Kelima chapter dan lima belas misi dapat dimainkan sampai epilog pada build Windows offline; semua Must dan TC wajib lulus.

- Struktur `Assets/_Game/`, daftar scene, prefab sekolah bersama dan lokasi data sesuai Bagian 16; build dimulai dari Bootstrap, seluruh level dapat dimuat, dan tidak ada referensi hilang atau objek inti ganda.


- Tidak ada bug kritis atau mayor terbuka. Kritis: crash, data hilang, alur buntu, atau pesan perlindungan dilanggar. Mayor: interaksi inti tidak berfungsi meski ada jalan memutar.

- Performa, kompatibilitas dan keterbacaan diukur pada perangkat yang dicatat; keterbatasan yang tersisa ditulis dalam catatan rilis.

- Review isi, hasil uji penggunaan dan evaluasi pembelajaran dilaporkan sesuai pelaksanaan aktual, termasuk temuan yang belum memenuhi target.

- Paket berisi build, README, versi PRD/GDD, naskah, daftar aset/lisensi, matriks hasil uji dan instrumen yang dipakai pada TA.

Penanggung jawab produk dan implementasi adalah pengembang TA; pembimbing mengarahkan lingkup serta evaluasi; guru BK atau penelaah kompeten memeriksa ketepatan pesan. Peran ini tidak berarti persetujuan sudah diberikan; status review dicatat selama pelaksanaan.

## 23 Dasar rancangan dan istilah

### Acuan rancangan

Acuan struktur implementasi adalah tangkapan layar panel Project Unity yang memperlihatkan root `Assets/_Game/`, folder aset dan sistem, serta pemisahan Bootstrap, MainMenu, Character Setup, env_design, Level_01–Level_05 dan Test. Nama file scene, subfolder modul serta nama prefab dalam Bagian 16 merupakan usulan penempatan yang perlu dicocokkan dengan file aktual.

Acuan cerita adalah rancangan terbaru lima chapter dan lima belas misi: Hari Pertama dan Sisi yang Tak Terlihat, Di Balik Tawa di Lapangan, Bangku yang Kosong, Tekanan di Balik Tugas, serta Pemulihan dan Perubahan. Catatan bimbingan menekankan pesan yang ingin disampaikan, bentuk perundungan, penyampaian dalam cerita, solusi dan ending.

Rujukan berikut mendukung penelaahan tema serta penanganan pada dokumen riset pendamping. Mekanik, ambang performa dan target evaluasi dalam PRD merupakan kebutuhan produk yang harus diuji; tidak dinyatakan sebagai temuan atau rekomendasi langsung dari seluruh penelitian ini.

### Rujukan penelitian pendamping

1. Xiong, M., Guo, X., & Ren, P. (2025). Are peer-rejected adolescents more likely to become bullies or victims? A longitudinal social network analysis. Journal of Youth and Adolescence, 54, 955–966. https://doi.org/10.1007/s10964-024-02110-7

2. Grama, D. I., Georgescu, R. D., Coşa, I. M., & Dobrean, A. (2024). Parental risk and protective factors associated with bullying victimization in children and adolescents: A systematic review and meta-analysis. Clinical Child and Family Psychology Review, 27, 627–657. https://doi.org/10.1007/s10567-024-00473-8

3. Chu, X., & Chen, Z. (2025). The associations between parenting and bullying among children and adolescents: A systematic review and meta-analysis. Journal of Youth and Adolescence, 54, 928–954. https://doi.org/10.1007/s10964-024-02108-1

4. Hensums, M., dkk. (2023). What works for whom in school-based anti-bullying interventions? An individual participant data meta-analysis. Prevention Science. https://doi.org/10.1007/s11121-022-01387-z

5. Zaneva, M., dkk. (2025). Social norms predict bullying: Evidence from an anti-bullying intervention trial in Indonesia. International Journal of Bullying Prevention. https://doi.org/10.1007/s42380-023-00176-8

6. Nagar, P. M., & Talwar, V. (2023). The role of teacher support in increasing youths’ intentions to disclose cyberbullying experiences to teachers. Computers & Education, 207, 104922. https://doi.org/10.1016/j.compedu.2023.104922

Catatan penerapan: penelitian pengungkapan cyberbullying tidak otomatis membuktikan efektivitas mekanik pelaporan tatap muka dalam game. Penelitian asosiasi keluarga juga tidak menetapkan penyebab tunggal perilaku Dion; latar tokoh tetap merupakan konstruksi naratif yang perlu ditelaah.

### Istilah kerja

PRD: persyaratan produk dan penerimaannya. GDD: rancangan pengalaman dan aturan game. Bystander: saksi kejadian. Checkpoint: titik pemulihan progres. Vertical slice: potongan permainan yang mewakili seluruh sistem inti. Soft lock: kondisi permainan masih berjalan tetapi alur tidak dapat diteruskan. Follow-up: pemeriksaan dan tindakan lanjutan setelah penanganan awal.
