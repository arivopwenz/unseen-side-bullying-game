# Sistem Prolog dan Level 01 yang Terimplementasi

**Proyek UNSEEN SIDE**  
**Tanggal audit 18 November 2026**  
**Engine Unity 6000.6.3f1**

Dokumen ini merekam sistem yang tersedia berdasarkan audit source, aset data, dan scene tersimpan melalui Unity MCP. Hasil Play Mode yang disebutkan berasal dari pengujian pada percakapan pengembangan sebelumnya. Audit tanggal ini membaca konfigurasi dan source terkini. Pengujian ulang penuh atas semua perubahan terbaru belum dilakukan.

## 1. Ringkasan status

| Bagian | Kondisi teramati |
|---|---|
| Prolog di mobil | Scene, Timeline, dan panel dialog berbasis TextMeshPro tersedia |
| Prolog keluar mobil | Scene terpisah dan Timeline turun mobil tersedia |
| Rangkaian Prolog hari pertama sesuai PRD V4 | Integrasi berurutan sampai tur, Sikka, logo, dan Level 01 belum terverifikasi |
| Level 01 hari kedua | Controller quest, data tujuh langkah, companion Bima, cutscene kantin, dua kartu, laporan guru, dan checkpoint terpasang |
| Bekal dan botol sebelum kantin | Source dan komponen terpasang untuk Bima mengambil serta membawa barang dari kelas |
| Cutscene kantin | Scene additive dengan kamera render dan lingkungan sendiri |
| Ambil dan serahkan botol | Satu objek gameplay berpindah dari dunia ke grip Aji lalu ke grip Bima |
| Gamepad | Binding gerak/kamera tersedia, navigasi panel dan kontrol Level 01 sudah diuji dengan perangkat virtual |
| Petunjuk kontrol dinamis | Terpasang pada HUD, bantuan jeda, dan petunjuk konfirmasi Kartu Kata Level 01 |
| Lingkungan sekolah | Prefab bersama dengan penggantian vegetasi low-poly dan penghapusan dua gawang |

## 2. Scene dan Timeline

Semua path dalam dokumen ini relatif terhadap root repository.

| Scene | Path | Director aktif saat audit | Aset Timeline |
|---|---|---|---|
| Prologue | `Assets/_Game/Scenes/Levels/Prologue/cutscene/Prologue.unity` | `TIMELINE`, Play On Awake aktif | `Assets/_Game/Scenes/Levels/Prologue/cutscene/Scene Prologue/Prologue.playable` |
| Prologue Keluar Mobil | `Assets/_Game/Scenes/Levels/Prologue/cutscene/Prologue Keluar Mobil.unity` | `TIMELINE (1)`, Play On Awake aktif | `Assets/_Game/Scenes/Levels/Prologue/cutscene/Scene Prologue/Prologue Turun mobil.playable` |
| Level01 | `Assets/_Game/Scenes/Levels/Level_01/Level01.unity` | Alur utama dikelola controller di `questLevel01` | Memanggil Timeline pada scene BekalCutscene |
| BekalCutscene | `Assets/_Game/Scenes/Levels/Level_01/cutscene/BekalCutscene.unity` | `Timeline Cutscene/Timeline Canteen`, Play On Awake nonaktif | `Assets/_Game/Scenes/Levels/Level_01/cutscene/Scene Canteen/Canteen.playable` |

`Ch1_BotolBima.unity` tersedia di folder cutscene Level 01. Alur ambil dan serahkan botol yang dibahas di dokumen ini berlangsung di gameplay Level01.

Build Settings yang diaudit masih mencantumkan Bootstrap, MainMenu, Level01–Level05, cutscene perkenalan kelas, dan BekalCutscene. Kedua scene Prolog di atas belum tercantum dalam daftar tersebut. Integrasi menu menuju rangkaian Prolog memerlukan pemeriksaan lanjutan.

## 3. Sistem Prolog

### 3.1 Dialog mengikuti Timeline

Tiga script berada di `Assets/_Game/Scripts/Cutscenes/Dialogue/` dengan namespace `UnseenSide.Cutscenes`.

| Script | Tanggung jawab |
|---|---|
| `PrologueDialogueClip.cs` | Menyimpan speaker, kalimat dialog, dan label urutan pada setiap clip |
| `PrologueDialogueTrack.cs` | Mengikat track ke panel dan memilih clip dengan bobot tertinggi |
| `PrologueDialoguePanel.cs` | Mengisi teks, mengatur warna speaker dan alpha panel, serta menyembunyikan dialog saat Director berhenti |

Mixer memberi fade masuk sekitar 0,12 detik dan fade keluar sekitar 0,18 detik pada dialog. Ketika tidak ada clip aktif atau graph berhenti, panel disembunyikan. Panel bersifat presentasi dan tidak memblokir raycast UI.

Warna speaker AJI memakai biru muda. Speaker lain memakai warna kuning hangat.

### 3.2 Font yang terpasang

Pada scene `Prologue.unity`, teks Speaker memakai `Super Bouncer SDF`, sedangkan Dialogue memakai `Cleanow SDF`. Aset TMP dibuat dari TTF di `Assets/_Game/fonts/`.

Pada scene `Prologue Keluar Mobil.unity`, Speaker dan Dialogue masih memakai `LiberationSans SDF` pada saat audit. Penyesuaian font kedua scene perlu dilakukan secara terpisah bila desainnya hendak diseragamkan.

### 3.3 Batas implementasi Prolog

Keberadaan scene dan Timeline turun mobil sudah dikonfirmasi. Handoff otomatis dari dialog mobil, pertemuan Bima, penghadangan, kelas, insiden Radit, tur sekolah, Sikka, dan logo menuju hari kedua belum diverifikasi sebagai satu rangkaian. Dokumen ini tidak memberi status selesai pada keseluruhan Prolog PRD V4.

## 4. Pemilik alur Level 01

GameObject `questLevel01` memiliki komponen aktif berikut.

| Script | Lokasi source | Fungsi |
|---|---|---|
| `LevelOneDirector` | `Assets/_Game/Scripts/Core/LevelOne/LevelOneDirector.cs` | Progres langkah, quest, interaksi, modal, simpan, dan prompt |
| `LevelOneView` | `Assets/_Game/Scripts/UI/LevelOneView.cs` | HUD, dialog, kartu, puzzle, jurnal, dan jeda |
| `LevelOneDayTwoFlow` | `Assets/_Game/Scripts/Events/LevelOneDayTwoFlow.cs` | Bima, pickup bekal, pintu, trigger kantin, kamera dan perpindahan additive |
| `LevelOneIncidentProps` | `Assets/_Game/Scripts/Events/LevelOneIncidentProps.cs` | Staging bekal dan botol sebelum, selama, dan setelah insiden |
| `LevelOneBottleGrab` | `Assets/_Game/Scripts/Inventory/LevelOneBottleGrab.cs` | Gerakan tangan dan perpindahan botol Aji–Bima |
| `LevelOneItemVisuals` | `Assets/_Game/Scripts/Inventory/LevelOneItemVisuals.cs` | Visibilitas barang mengikuti progres |

Interaksi lokasi memakai `LevelOnePoint` di `Assets/_Game/Scripts/Interaction/LevelOnePoint.cs`. Quest tetap memakai sistem `QuestManager` yang sudah ada.

Komponen pendukung Fatimah `LevelOneCompanion` dan anchor petugas kantin masih aktif di scene saat audit. Root quest juga masih memiliki titik dari versi alur sebelumnya yang sebagian dinonaktifkan.

## 5. Urutan gameplay hari kedua

Data alur berada di `Assets/_Game/Data/LevelOnePRD/Level01_HariKedua.asset`.

| Urutan | Step ID | Point ID | Perilaku |
|---|---|---|---|
| 1 | `greet` | `MeetBima` | Bel istirahat, sapa Bima di dalam kelas |
| 2 | `incident` | `Canteen` | Tunggu Bima mengambil bekal dan botol, kemudian ikuti menuju kantin |
| 3 | `clue` | `BimaCanteen` | Kartu Aksi dipresentasikan, lalu tanyakan lokasi botol |
| 4 | `bottle` | `P3` | Periksa petunjuk dan ambil botol |
| 5 | `return` | `Bench` | Serahkan botol kepada Bima |
| 6 | `words` | `SafeWords` | Susun respons aman kepada Memet dan Raka |
| 7 | `report` | `Teacher` | Temui Bu Ratna bersama Bima, lanjut refleksi |

Pada hierarki terkini, point P3 memiliki nama `P3 - KotakMakan an Botol Bima`. Script memakai ID `P3`, sehingga label hierarki dapat berbeda dari ID interaksi.

Penyelesaian Level 01 ditentukan oleh indeks langkah yang sudah melewati seluruh array data. Status ini disimpan bersama checkpoint.

### 5.1 Companion dan pintu

Bima memakai NavMeshAgent. Ia mengambil barang dari pickup spot melalui `LevelOneIncidentProps.PickUp()`, kemudian menandai `bimaPackedLunch` dan menyimpan progres. Saat menuju kantin, Bima menunggu bila Aji berjarak lebih dari 10 meter.

Perhitungan tujuan memakai filter NavMesh sesuai tipe agen Bima. Referensi `classroomExit` dan `reportClassroomDoor` membuka pintu terdekat melalui `SwingDoor`. Saat pelaporan, Bima mengikuti Aji.

Trigger kantin mensyaratkan langkah incident, Bima dekat penanda, Aji dapat menjangkau titik, serta barang sudah dibawa bila controller props tersedia. Interaksi serah-terima memerlukan Bima dekat titik return. Laporan memerlukan Bima dekat titik guru.

## 6. Cutscene kantin dan isolasi environment

`LevelOneDayTwoFlow` memuat BekalCutscene secara additive. Urutan penanganannya sebagai berikut.

1. Input gameplay dibekukan dan state menjadi Cinematic.
2. Renderer lingkungan Level01 disembunyikan sementara melalui `forceRenderingOff`. Lampu, Volume, reflection probe, dan collider lingkungan yang aktif dijeda. NavMeshSurface gameplay tetap dimuat.
3. Visual aktor gameplay yang terikat di `cinematicStandIns` disembunyikan.
4. Kamera render, Brain, dan AudioListener gameplay dijeda.
5. BekalCutscene dimuat, dijadikan active scene, dan pengaturan grafis lingkungan lokal diterapkan.
6. Director memakai binding CinemachineBrain pada kamera render milik BekalCutscene.
7. Timeline berjalan hingga berhenti secara alami. Ada batas waktu durasi Timeline ditambah lima detik.
8. Active scene dikembalikan ke Level01. Scene cinematic dilepas, lingkungan dan kamera gameplay dipulihkan.
9. Jika sukses, alur maju ke clue. Kegagalan menampilkan pesan untuk mencoba lagi dan tidak menyelesaikan langkah insiden.

Posisi AreaSekolah pada kedua scene dapat berbeda. Pemisahan visibilitas diperlukan karena kedua scene berbagi koordinat dunia selama additive.

### 6.1 Props pada Timeline

BekalCutscene saat audit memiliki `BekalCutsceneProps` aktif pada `Timeline Canteen`. `BekalPropsTrack` dan `BekalPropsClip` mendukung evaluasi props mengikuti waktu Timeline.

Controller props mengalihkan visibilitas dari bekal/botol Bima ke bekal Memet dan botol Raka pada waktu transfer yang terserialisasi. Nilai default di source `BekalCutsceneProps` adalah 28,05 detik. Nilai Inspector harus dicek ketika timing Timeline diubah.

Bila track props lokal terkonfigurasi, `LevelOneIncidentProps` menyembunyikan props gameplay selama cinematic. Setelah insiden, props gameplay dipulihkan dan botol ditempatkan pada `bottleRecovery` bila belum diambil. Posisi ini menjadi referensi terkini saat mengedit penempatan botol.

## 7. Kartu Aksi dan Kartu Kata

### 7.1 Presentasi Kartu Aksi

Hierarki presentasi tersimpan di bawah questLevel01.

```text
questLevel01
└── Canvas_KartuAksi
    ├── Backdrop
    └── Image_KartuAksi
```

Image menggunakan sprite `Assets/_Game/Art/UI/card-game/kartu-aksi.png`. Canvas overlay menampilkan pop-up dan dua lintasan flip, lalu artwork menghilang sebelum panel penjelasan misi muncul. Tombol Lanjut mengembalikan kontrol dan menyimpan flag `actionCardShown`.

Durasi animasi nominal sekitar 2,28 detik. Penambahan waktu animasi dibatasi saat frame lambat agar lintasan flip tetap ditampilkan, sehingga durasi nyata dapat lebih panjang.

### 7.2 Puzzle Kartu Kata

Data puzzle berada di `Assets/_Game/Data/LevelOnePRD/WRD_L01_01.asset`. Pemain mengisi tiga slot dari lima keping.

Jawaban aman yang digunakan berurutan.

1. Memet, Raka, tolong berhenti.
2. Bima perlu ruang.
3. Kalau ada masalah, kita bisa bicara sama Bu Ratna.

Bank keping diacak dan urutannya disimpan. Setiap keping hanya digunakan sekali. Periksa aktif setelah tiga slot terisi. Jawaban salah memberi feedback dan tetap berada di puzzle. Jawaban benar memutar dialog hasil, lalu membuka laporan ke guru.

Susun ulang mengosongkan slot. Kembali menutup puzzle sambil menyimpan pilihan. Saat panel aktif, input gerak dan kamera gameplay dimatikan melalui daftar `modalControls`.

## 8. Grab dan serah-terima botol

Botol gameplay memakai objek yang terikat pada Inspector. Alur status yang relevan adalah world, bottleFound, bottleTaken, kemudian returned.

- Inspeksi dan pickup merupakan interaksi terpisah.
- Pickup menjalankan reach tangan Aji dan memindahkan parent botol ke `Grip_BotolBima` di tangan Aji.
- Botol mengikuti tangan saat dibawa.
- Serah-terima menghadapapkan kedua karakter dan menjalankan reach, lalu memindahkan botol ke grip Bima.
- Botol tetap di Bima selama dialog serah-terima. `returned` dikomit setelah dialog selesai.
- Restore checkpoint mengembalikan kepemilikan visual sesuai progres.

Gerakan lengan dihitung secara prosedural pada LateUpdate menggunakan tulang lengan dan tangan. Model Aji dengan rig tangan lengkap dipasang pada Level01 untuk mendukung mekanik ini. Penyesuaian pose dan offset grip dapat dilakukan melalui referensi rig di Inspector.

## 9. Input dan prompt dinamis

Karakter memakai Input System melalui `GameInputActions`. Asset utama berada di `Assets/_Game/Settings/GameInput.inputactions`. UI memakai EventSystem dan `InputSystemUIInputModule`.

| Aksi | Keyboard/mouse | Xbox | PlayStation |
|---|---|---|---|
| Gerak | WASD | Stik kiri | Stik kiri |
| Kamera | Mouse | Stik kanan | Stik kanan |
| Lari | Left Shift | LB | L1 |
| Lompat | Space | A | Silang |
| Interaksi | E | X | Kotak |
| Jeda | Escape | Start | Options |
| Jurnal Level01 | Tab | View | Share/Create |
| Pilih UI | Navigasi UI existing | D-pad/stik kiri | D-pad/stik kiri |
| Konfirmasi UI | Binding Submit existing | A | Silang |
| Kembali pada panel yang mendukung | Escape/jeda | B | Lingkaran |

`ActiveInputDeviceTracker.cs` dan `InputPromptResolver` di folder Scripts/UI mendeteksi action yang digunakan dan membaca binding runtime, termasuk override. Koneksi controller saja mempertahankan prompt keyboard hingga perangkat dipakai. Controller generik memakai teks arah tombol.

Prompt Level01 terhubung ke HUD interaksi, bantuan jeda, jurnal, dan hint konfirmasi puzzle. Penerapan mekanisme yang sama pada keseluruhan Prolog belum diverifikasi. Label lain seperti petunjuk akhir level dan menu lama masih perlu audit konsistensi.

## 10. Checkpoint, jurnal, dan reset

`LevelOneCheckpoint` menyimpan JSON hari kedua dalam `Application.persistentDataPath/unseen-level01-day-two.json`, dengan versi data 4. Data legacy memakai file terpisah `unseen-level01-cards-v1.json`.

Data yang dicatat meliputi langkah, progres quest, posisi Aji dan Bima, status barang, flag kartu, slot dan urutan bank kata, percobaan puzzle, catatan jurnal, serta `bimaPackedLunch`.

Penulisan memakai file sementara dan penggantian file utama dengan backup `.bak`. Load mencoba file utama lalu backup. Kegagalan simpan ditampilkan pada HUD. Editor mendukung `LevelOnePRD.TestSave` pada SessionState untuk checkpoint uji terisolasi.

- Reset Level01 melalui menu Jeda → Ulang Level 1 → Ulang.
- New Game pada MainMenu mereset campaign dan checkpoint hari kedua.
- Continue mendeteksi save campaign atau progres hari kedua. Routing chapter tetap mengikuti data campaign dan konfigurasi chapter.

## 11. Lingkungan bersama

Prefab `Assets/_Game/Prefabs/Environment/AreaSekolah.prefab` telah diperbarui dengan model dari `Assets/_Game/assetEnv/_newassetlowpoly/`.

| Jenis | Jumlah pengganti |
|---|---|
| Pohon | 55 |
| Semak | 85 |
| Rumput | 18 |

Total segitiga vegetasi yang diganti turun dari 3.569.347 menjadi 1.154.338, sekitar 68%. Masing-masing model memakai material URP Lit dengan tekstur dari folder yang sama. Collider 55 pohon memakai kapsul batang. Dua objek gawang telah dihapus dari prefab. Collider peta dinding dan pot bunga pink telah disederhanakan menjadi box.

Angka pengurangan ini khusus vegetasi tersebut. Pengukuran FPS dan validasi seluruh instance scene terhadap override prefab masih diperlukan.

## 12. Panduan mengedit

### Mengedit Prolog

Buka scene Prologue yang sesuai, pilih Director TIMELINE, lalu edit clip pada track dialog atau track animasi. Font diubah pada label Speaker dan Dialogue milik Prologue Dialogue UI. Scene keluar mobil memiliki konfigurasi UI tersendiri.

### Mengedit cutscene kantin

Buka BekalCutscene, pilih Timeline Cutscene → Timeline Canteen, lalu aktifkan Preview dan scrub Timeline sambil melihat Game View. Scene memiliki kamera render lokal. Track Cinemachine menentukan shot. Sesuaikan transfer props melalui binding BekalPropsTrack dan referensi BekalCutsceneProps.

### Mengedit lokasi item dan quest

Edit point berdasarkan ID serta referensi Inspector. Untuk botol setelah insiden, periksa pula bottleRecovery pada LevelOneIncidentProps karena controller dapat memindahkan botol ketika runtime. Jaga referensi LevelOneItemVisuals, LevelOneBottleGrab, dan point P3 menuju objek yang dimaksud.

### Mengedit fade

Gunakan Canvas Screen Space - Overlay dengan Image hitam full stretch dan alpha yang dianimasikan. Saran tersebut sudah diberikan pada pengembangan sebelumnya. Audit dokumen ini belum membuktikan bahwa seluruh track Loading telah dikonversi ke pola fade ini.

## 13. Verifikasi dan pekerjaan lanjutan

### Pengujian sebelumnya yang tercatat

- Timeline Canteen menyelesaikan playback alami dan kembali ke langkah clue.
- Kamera render dan AudioListener lokal bekerja selama cutscene, lalu kamera gameplay pulih.
- Lingkungan Level01 tersembunyi selama cutscene, kemudian renderer, collider, dan NavMesh Bima tersedia kembali.
- Pickup, grip Aji, grip Bima selama dialog, satu botol gameplay, dan simpan progres serah-terima lolos.
- Kartu Aksi menampilkan dua flip sebelum panel misi.
- Kartu Kata mendukung jawaban salah, koreksi, simpan slot, dan reload sampai status selesai.
- Gamepad virtual menguji gerak, kamera, pause, jurnal, dialog, dan pemilihan keping puzzle.
- Prompt runtime diuji berpindah E → gamepad generik → E → override F → gamepad generik → fallback F saat perangkat dilepas.
- Penggantian vegetasi dan penghapusan gawang diverifikasi pada prefab tersimpan.

### Audit tanggal 18 November 2026

Audit ini memastikan keberadaan komponen aktif, Director, aset Timeline, font, source controller, dan urutan data quest terbaru. Perubahan pickup bekal di kelas, pintu, props track cinematic, dan konfigurasi scene terkini memerlukan Play Mode ulang end-to-end.

### Pekerjaan yang masih terbuka

- Verifikasi dan sambungkan rangkaian Prolog V4 sampai logo dan transisi hari kedua.
- Tambahkan scene Prolog yang diperlukan ke Build Settings dan audit routing menu.
- Seragamkan font scene keluar mobil bila desain menghendaki Super Bouncer/Cleanow.
- Uji controller Xbox/PlayStation fisik dan build Windows.
- Uji ulang path Bima, pintu, pickup bekal, serta props cinematic setelah perubahan staging terbaru.
- Uji recovery load/unload gagal dan interupsi cinematic.
- Audit dan rapikan komponen legacy berdasarkan referensi aktif.
- Validasi fade terbaru serta tampilan UI pada beberapa resolusi.
- Profil performa setelah penggantian vegetasi.

Script smoke lama seperti LevelOnePRDSmoke dan LevelOneExistingSmoke masih merekam asumsi alur terdahulu. Gunakan pemeriksaan yang sesuai dengan data hari kedua terkini sebelum menjadikan hasilnya sebagai bukti kelulusan.
