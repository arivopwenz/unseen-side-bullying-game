# Chapter 1 — Perkenalan karakter, POV korban, dan buku yang disembunyikan

Tanggal: 6 Oktober 2026. Catatan desain Chapter 1 dan tutorial Step 21.

> Pembaruan aktif 7 Oktober 2026: lihat [Implementasi Lima Chapter Outdoor](Five_Outdoor_Chapters_Implementation.md). Konfigurasi scene sekarang dikerjakan langsung melalui Editor sesuai izin terbaru pengguna; panduan manual di bawah tetap tersedia untuk belajar.

## 1. Arahan dari pengguna dan keputusan yang masih sementara

Arahan pengguna:
- Chapter pertama berisi beberapa misi yang mengenalkan karakter dan mekanik.
- Pembukaan memakai cutscene Timeline: kelompok pembully dengan ketua menjahili seorang teman cowok; karakter itu bukan Guru BK.
- Tiga POV menjadi karakter utama yang dimainkan: korban, pembully, dan bystander/saksi.
- Chapter pertama menonjolkan pengalaman korban, dengan pencarian barang, pemalakan/pungli, dan intimidasi.
- Seorang saksi diperkenalkan untuk menghubungkan chapter berikutnya.
- QTE gagal membuat barang disembunyikan di lokasi acak, bukan dikembalikan ke lokasi awal.
- Coding dikerjakan Codex. Konfigurasi scene, Timeline, kamera, UI, dan penempatan objek dilakukan pengguna secara manual.

**Asumsi sementara yang perlu dikonfirmasi:** pemain Chapter 1 adalah korban utama. Teman cowok yang dijahili pada pembukaan adalah korban lain. Buku yang diminta untuk dicari adalah milik ketua pembully, sesuai kalimat pengguna tentang membantu pembully. Bila pemain sebenarnya adalah teman penolong atau cowok pada pembukaan, hubungan karakter dan dialog harus disesuaikan sebelum scene cerita dibuat. Nama karakter di bawah adalah label peran, belum nama final.

Menangis adalah respons terhadap tekanan, bukan alasan seseorang layak dirundung. Dalam dialog dan HUD gunakan nama karakter atau “teman”, bukan label “si cengeng”. Pemain yang kalah QTE tetap mendapat jalan untuk melanjutkan cerita dan mencari dukungan.

## 2. Yang sudah ada dan yang masih direncanakan

| Bagian | Status |
|---|---|
| Gerak, kamera, interaksi, dialog, quest dasar, HUD | Sudah ada |
| Pickup → Timeline → tiga pembully mendekat → dialog → QTE → gameplay | Sudah ada; pengguna telah mencoba |
| Encounter satu kali, objective tertunda sampai barang aman | Sudah ada |
| Buku berpindah acak ke anchor yang terjangkau ketika QTE benar-benar gagal | Coding Step 21 ditambahkan; pengguna perlu memasang komponen dan anchor |
| Teks setelah quest selesai bisa diatur pada QuestData | Coding ditambahkan; isi Completion Message manual |
| Opening cinematic Chapter 1 | Rancangan; Timeline baru belum dibuat |
| Rangkaian misi, validasi prerequisites, serah barang, pergantian objective otomatis | Rancangan; belum diimplementasikan |
| Percabangan pilihan, dukungan teman, refleksi, dan perubahan POV | Rancangan; belum diimplementasikan |
| Animasi tangan merampas/mempertahankan buku | Rancangan Step 23; prototype sekarang memindahkan objek setelah hasil |
| Save/checkpoint dan penyimpanan event antar-chapter | Rancangan; state sekarang hanya berlaku pada sesi berjalan |

Jangan menganggap mengisi `prerequisites` di asset sudah menjalankan quest berantai. Field tersebut memang ada, tetapi QuestManager saat ini belum memeriksanya. QuestGiverNPC saat ini juga hanya memiliki satu quest dan tiga cabang dialog. Menambahkan banyak asset saja belum menghasilkan rangkaian misi ini.

## 3. Peran karakter

| Peran | Fungsi Chapter 1 | Fungsi chapter mendatang |
|---|---|---|
| Korban utama / Player | Memahami tekanan, mempertahankan batas, membantu teman, mencari dukungan | POV korban dengan konsekuensi emosional dan pemulihan |
| Ketua pembully | Memimpin dua anggota; meminta pencarian buku kemudian menuduh/menekan pemain | Calon karakter POV pembully: memahami pilihan, dampak, dan tanggung jawab |
| Dua anggota | Menguatkan tekanan kelompok; punya sikap dan respons berbeda | Menunjukkan dinamika kelompok, bukan tiga karakter identik |
| Teman cowok | Target pada pembukaan; kemudian menghadapi pemalakan dan intimidasi | Hubungan dengan Player berkembang; punya keinginan dan keputusan sendiri |
| Saksi | Melihat kejadian, ragu ikut campur, lalu memberi bantuan kecil yang konkret | Calon karakter POV bystander pada chapter berikutnya |
| Orang dewasa tepercaya | Pilihan dukungan setelah kejadian, misalnya wali kelas/Guru BK | Mendukung tindak lanjut; bukan korban pada cutscene pembukaan |

Guru BK boleh tetap ada sebagai pihak pendukung. Posisinya dalam cerita harus dibedakan dari NPC teman yang dirundung dan NPC pemberi misi buku. Jangan sekadar mengganti nama speaker di semua asset tanpa mengganti konteks percakapannya.

## 4. Bentuk Chapter 1 yang diusulkan

Target prototype: satu area sekolah kecil, enam misi utama, sekitar 15–20 menit setelah pacing diuji. Estimasi ini adalah sasaran desain, bukan hasil pengukuran. Satu tujuan utama tampil di HUD pada satu waktu.

Alur:

```text
Opening: ketua dan dua anggota menjahili teman cowok
  → M01 Temui teman dan pahami situasinya
  → M02 Cari buku yang diminta ketua pembully
      → ambil buku → encounter notebook → QTE
          berhasil: buku aman
          gagal: buku tersembunyi acak → cari lagi → buku aman
      → kembalikan buku lewat dialog, bukan langsung akhir chapter
  → M03 Temui teman yang menghadapi pemalakan
  → M04 Dampingi teman melewati intimidasi
  → M05 Dengar kesaksian dan cari dukungan
  → M06 Refleksi singkat dan pengantar POV saksi
```

### Opening — “Sebelum pelajaran dimulai”

Timeline usulan 20–30 detik: shot lingkungan sekolah, close-up ketua memegang barang teman, dua anggota menekan secara verbal, teman meminta barangnya kembali, reaksi Player, dan shot singkat saksi yang melihat tetapi belum bergerak. Akhiri dengan tujuan “Temui temanmu”. Durasi dan framing ditentukan setelah blockout.

Opening memakai PlayableDirector/Timeline sendiri. Jangan menjalankannya melalui pickup buku atau memakai Event ID notebook: pembukaan tidak boleh menghabiskan encounter satu kali milik buku. Kontrol kembali setelah Timeline berhenti, di-skip, atau batal. Skip tetap membawa pemain ke tujuan pertama.

Kamera berasal dari Cinemachine; Main Camera tetap digunakan melalui Brain. Animasi yang dimainkan Timeline dan NavMeshAgent tidak boleh sekaligus menggerakkan karakter yang sama. Kepemilikan gerak dipulihkan setelah cutscene.

### M01 — “Kamu tidak sendirian”

Tujuan: temui teman, dengarkan ceritanya, lalu sepakati langkah kecil untuk melanjutkan hari. Kenalkan berjalan, kamera, prompt interaksi, dan membaca dialog. Saksi diperlihatkan di lingkungan sebelum menjadi tokoh percakapan penting.

Dialog usulan:
- Teman: “Aku sudah bilang berhenti. Mereka malah ketawa.”
- Player: “Aku dengar kamu. Mau aku temani dulu?”
- Teman: “Iya. Jangan ramai-ramai dulu, ya.”

Selesai setelah percakapan selesai secara normal. Menutup UI atau membatalkan dialog tidak dihitung sebagai mendengarkan sampai selesai. Pilihan respons bercabang belum didukung DialogueData yang sekarang; mulai dengan dialog linear dahulu.

### M02 — “Buku dan tekanan”

Ketua meminta Player mencari buku miliknya, sambil menyiratkan ancaman. Player dapat menolong seseorang tanpa menyetujui perilakunya. Di dekat buku, kelompok mendekat dan menuduh Player menyembunyikannya; ini tindakan menekan yang tidak adil, bukan kesalahan Player.

Objective final yang direncanakan:
1. `find_notebook`: buku benar-benar aman.
2. `return_notebook`: selesai percakapan pengembalian dengan pemiliknya.

Prototype sekarang hanya memiliki `find_notebook`; karena itu HUD bisa menyatakan selesai sesaat setelah buku aman. Completion Message hanyalah petunjuk setelah selesai, belum menggantikan objective serah barang. **Jangan menambahkan `return_notebook` sekarang sebelum handler pengembalian dibuat**, karena quest akan tertahan pada objective yang belum punya pemicu.

QTE mempertahankan buku, bukan berkelahi. Berhasil → objective buku aman sekali. Gagal → objective tetap 0, buku dipindahkan ke anchor acak, pemain mencarinya lagi. Pickup kedua mengamankan buku tanpa mengulang pembully atau QTE.

Untuk tahap berikutnya, beri petunjuk area pencarian lewat saksi atau dialog singkat. Jangan menambahkan marker yang menunjukkan posisi tepat kecuali sebagai opsi bantuan. Area pencarian kecil dan jelas; empat anchor cukup untuk prototype, tanpa timer pencarian kedua.

### M03 — “Uang jajan bukan kewajiban”

Teman dipalak. Pemain mengetahui permintaannya lewat percakapan, lalu membantu memilih langkah aman: menemani teman, mengingat rincian kejadian, atau menghubungi orang dewasa tepercaya. Intervensi langsung bukan satu-satunya respons yang benar.

Prototype awal: percakapan linear → temani teman ke lokasi aman → lapor lewat percakapan. Pilihan bercabang dan flag konsekuensi dibangun setelah quest berantai bekerja. Jangan memakai QTE yang memberi pesan bahwa kekuatan fisik menentukan siapa yang benar. Bila ada uang yang sudah diambil, tujuan adalah bantuan dan tindak lanjut, bukan mengharuskan korban membeli kebebasan.

### M04 — “Jangan pisahkan kami”

Kelompok mengintimidasi Player karena membantu teman. Bedakan mekanik dari pencarian buku: berjalan bersama teman ke titik aman dengan pacing pendek, diselingi dialog, bukan koleksi barang lagi. Jalur sederhana tanpa gang buntu.

Gagal/terhambat pada fase pendampingan memberi checkpoint dekat dan alternatif mencari bantuan. Hindari mengulang seluruh chapter. Pengikut teman memerlukan komponen pendamping sendiri; jangan menjadikan BullyGroupController pengatur semua NPC karena perannya berbeda.

### M05 — “Yang dilihat saksi”

Saksi mengakui melihat kejadian dan menjelaskan keraguannya. Player dan teman dapat menerima dukungan saksi. Contoh kontribusi: menyebut waktu/lokasi, menemani ke pihak tepercaya, atau menguatkan bahwa kejadian itu benar terjadi. Kesaksian bukan syarat agar korban pantas dipercaya.

Objective: dengarkan saksi → temui pihak dukungan → rencanakan tindak lanjut. Guru BK dapat masuk di sini sebagai pihak pendukung. Percakapan tidak langsung menghapus dampak atau menyelesaikan semua masalah secara ajaib.

### M06 — “Besok, dari sisi lain”

Refleksi 2–3 pertanyaan singkat tentang tekanan, pilihan aman, dan peran saksi. Tidak menilai korban berdasarkan menang/kalah QTE. Penutup memperlihatkan saksi mengingat keputusan untuk diam tadi, sebagai pengantar chapter bystander.

Sistem quiz/refleksi dan perpindahan chapter belum dibuat. Mulai dengan dialog penutup sebagai blockout, kemudian implementasikan sistemnya sesuai roadmap.

## 5. Tiga POV yang terhubung

Usulan urutan: Chapter 1 korban → Chapter 2 saksi → Chapter 3 pembully. Urutan ini bisa berubah setelah naskah disepakati. Jangan membuat tiga versi kejadian yang saling bertentangan.

Simpan fakta bersama dengan ID, misalnya `ch01_opening`, `ch01_notebook`, `ch01_extortion`, `ch01_intimidation`. Tiap POV memperlihatkan informasi yang berbeda tentang fakta yang sama. Catat siapa yang hadir, apa yang dilihat, waktunya, dan tindakan yang benar-benar terjadi.

- Korban: tekanan, ketidakpastian, pilihan mencari bantuan, hubungan dengan teman.
- Saksi: ragu karena takut menjadi target, lalu belajar tindakan kecil dan dukungan nyata.
- Pembully: pilihan di dalam kelompok, pengaruh lingkungan, dampak pada orang lain, dan tanggung jawab memperbaiki tindakan. Latar belakang menjelaskan perilaku tanpa membenarkannya.

Saat POV berpindah kelak, karakter yang sama tidak perlu diduplikasi menjadi dua orang. Chapter/POV controller mengatur karakter yang menerima input dan kamera; NPC lainnya tetap berjalan sebagai NPC. Jangan menambahkan tiga PlayerInputHandler aktif sekaligus. Implementasi controller ini masih menunggu Step 24.

## 6. Kontrak teknis yang perlu dibangun berikutnya

1. **Quest berantai:** validasi prerequisites, aktifkan satu misi utama berikutnya, catat penyelesaian satu kali. Setiap quest/objective/event punya ID unik.
2. **Serah barang:** simpan fakta buku aman di data runtime; jangan memeriksa ada/tidaknya GameObject karena buku dapat dihancurkan setelah diamankan. Dialog pengembalian normal menambah `return_notebook` sekali. Pembatalan tidak menambah progress.
3. **NPC dan konteks dialog:** quest milik ketua dibedakan dari bantuan kepada teman. Pendamping, saksi, dan pembully mempunyai fungsi masing-masing.
4. **Chapter controller:** urutan opening → misi → penutup, termasuk skip/batal dan checkpoint. Timeline hanya mengatur presentasi; quest tetap sumber kebenaran progress.
5. **Pilihan dan fakta cerita:** data pilihan, kondisi percakapan, flag dukungan, dan hasil keputusan; belum tersedia pada dialog linear saat ini.
6. **Save/checkpoint:** simpan quest progress, event yang sudah ditangani, posisi/ID anchor buku, dan fakta cerita. Jangan mengacak ulang barang yang sudah disimpan saat load. HashSet event sekarang belum persisten.

Bangun satu misi sampai selesai dan uji dahulu sebelum menghubungkan keenamnya. Roadmap Step 22 kamera, Step 23 animasi, dan Step 24 POV tetap berlaku; desain beberapa misi tidak berarti semuanya sudah diimplementasikan pada Step 21.

## 7. Coding Step 21: perilaku buku sekarang

Komponen baru: `BullyingGame.Quest.QuestItemRehide`, ditempel pada GameObject yang sama dengan QuestItem.

- `Hide Points`: daftar Transform kosong yang ditempatkan manual. Posisi anchor adalah titik tengah item, bukan titik kaki karakter.
- `Minimum Move Distance`: jarak horizontal minimum dari posisi buku sebelum dipindahkan; default 2 meter.
- `Require Reachable Path`: default aktif. Memeriksa koneksi pada NavMesh; jangan dimatikan hanya untuk melewati kesalahan bake.
- `Agent Type Id`: ID navigation agent; default 0 untuk setup Humanoid yang sekarang.
- `Sample Radius`: default 0.75 meter, toleransi pencarian NavMesh di dekat anchor.
- Anchor null, tidak aktif, anak dari buku, berulang pada posisi yang sama, terlalu dekat, di luar NavMesh, atau terputus dari jalur buku diabaikan.
- Posisi yang dipakai tetap posisi anchor; tinggi tidak diganti dengan tinggi NavMesh. Pemeriksaan jalur belum memeriksa penempatan mesh buku terhadap dinding/meja, sehingga penempatan fisik tetap perlu dites manual.
- Ketika tidak ada anchor valid, muncul warning dan buku ditampilkan kembali agar quest tidak terkunci. Ini fallback konfigurasi, bukan perilaku final yang diinginkan.
- QTE timeout adalah kalah permainan dan memindahkan buku. Cancel QTE, skip cutscene, atau setup yang tidak bisa berjalan mengembalikan buku tanpa mengacak.
- Renderer buku disembunyikan selama encounter. Root/collider ikut pindah; item ditampilkan dan dapat diinteraksikan setelah kembali ke Playing.
- Encounter sudah dianggap ditangani, sehingga pengambilan berikutnya tidak memutar encounter yang sama.

State item: Available → Collected (menunggu hasil) → Secured, atau Collected → Stolen → Rehidden → Secured. State terpisah dari renderer dan progress quest. Hidden disediakan untuk pengembangan mekanik awal tersembunyi; belum dipakai pada pickup ini.

## 8. Konfigurasi manual Unity untuk Step 21

Kerjakan di Edit Mode. Tunggu kompilasi selesai, lalu simpan scene.

1. Pilih `_Level`, Create Empty, nama `NotebookHidePoints`. Set local Position/Rotation `(0,0,0)`, Scale `(1,1,1)`.
2. Buat empat child Empty: `Hide_A`, `Hide_B`, `Hide_C`, `Hide_D`. Jangan taruh sebagai child NotebookItem.
3. Untuk ground datar prototype dengan buku awal `(3,0.1,1.5)`, gunakan contoh **world position** berikut. Pastikan ruang kosong dan dapat didatangi Player:

| Anchor | X | Y | Z |
|---|---:|---:|---:|
| Hide_A | -4 | 0.1 | 4 |
| Hide_B | 7 | 0.1 | -3 |
| Hide_C | -6 | 0.1 | -4 |
| Hide_D | 1 | 0.1 | 8 |

4. Pilih `_Level/NotebookItem`. Add Component → cari `Quest Item Rehide`. Komponen yang benar memiliki Hide Points dan Require Reachable Path.
5. Hide Points Size = 4; drag masing-masing **Empty dari Hierarchy**, bukan Player/buku/script dari Project, ke Element 0–3. Tidak ada komponen script yang perlu ditambahkan ke anchor.
6. Biarkan Minimum Move Distance = 2, Require Reachable Path aktif, Agent Type Id = 0, Sample Radius = 0.75.
7. Pada `_Level/Environtment`, periksa NavMeshSurface dan visualisasi NavMesh. Semua lokasi harus berada di area lantai yang terhubung. Bila geometri lantai berubah, Bake kembali. Anchor kosong saja tidak mengubah geometri bake.
8. Pilih asset Quest_Level01_LostNotebook di Project. Isi Completion Message sesuai NPC pemilik yang sudah kamu putuskan, misalnya “Kembalikan buku kepada ketua kelompok.” Ini hanya teks HUD; jangan menambah objective pengembalian dahulu.
9. Simpan scene dan asset di luar Play Mode.

Tes manual:
- Mulai sesi Play baru, aktifkan quest, ambil buku, sengaja biarkan timer QTE habis.
- Setelah gameplay kembali, buku tidak ada di posisi awal. Pilih NotebookItem di Hierarchy untuk melihat posisinya; nilainya harus sama dengan salah satu anchor.
- Quest masih aktif, progress buku 0/1, kamera dan gerak normal.
- Datangi posisi baru dan ambil buku: progress 1/1, tidak ada encounter/QTE kedua.
- Stop lalu Play untuk sesi baru; ulang beberapa kali. Acak boleh memilih anchor yang sama antar-sesi, tetapi selalu berbeda dari posisi awal. Ini bukan rotasi A → B → C → D.
- Tes sesi baru dengan QTE berhasil: item aman, tidak dipindahkan ke anchor.
- Jika tetap di posisi awal, periksa Console warning, komponen aktif, anchor aktif, jarak, dan NavMesh. Jangan mematikan QuestItem/root buku untuk menyembunyikan visual.

## 9. Urutan kerja manual cerita setelah spawn acak bekerja

1. Sepakati siapa Player, siapa teman cowok, dan siapa pemilik buku. Tetapkan nama karakter.
2. Buat/duplikasi asset dialog untuk teman dan ketua agar asset Guru BK yang lama tetap dapat dipakai sebagai dukungan. QuestGiverNPC dapat tetap dipakai pada NPC pemberi quest buku; isi speaker dan konteks yang sesuai.
3. Buat blockout teman cowok dan saksi sebagai GameObject terpisah dari tiga BullyNPC. Belum perlu NavMeshAgent jika mereka belum bergerak.
4. Buat opening Timeline terpisah setelah pemicu/controller opening disiapkan dalam coding berikutnya. Jangan menambah marker opening ke Timeline notebook.
5. Implementasikan dan konfigurasi M01 → M02 + pengembalian buku dahulu. Baru perluas ke pemalakan, pendampingan, dan saksi.
6. Perbaiki kamera dan animasi setelah urutan serta quest stabil. Art final dan detail lingkungan menyusul.

## 10. Catatan verifikasi

Verifikasi 6 Oktober 2026 pada Unity Editor 6000.6.3f1:

- Recompile selesai tanpa error kompilasi.
- **17 skenario Play Mode lulus** melalui fixture runtime sementara: approach → dialog → QTE berhasil/gagal; signal yang menunggu approach; cancel/skip QTE; dialog dibatalkan/manager atau UI dinonaktifkan; dialog/QTE data tidak valid; dialog biasa dan input yang sudah ditahan; filtering anchor; pilihan acak; larangan kembali ke posisi sekarang; serta Completion Message pada HUD.
- QTE gagal memilih anchor yang terhubung NavMesh dan berbeda dari posisi awal. Collider mengikuti posisi buku. Progress tetap 0 sampai pengambilan ulang, lalu menjadi 1 sekali tanpa encounter kedua.
- Dua anchor valid sama-sama terpilih pada 32 pengambilan acak dengan seed tes tetap; titik pada pulau NavMesh terpisah, child buku, null/tidak aktif, posisi duplikat, dan posisi sekarang diabaikan.
- QTE berhasil, cancel/skip, dan setup invalid tidak memindahkan buku. Kontrol, kamera, cursor, dan input action dipulihkan.
- Console aktual setelah tes tidak memiliki error. Warning dari skenario data/anchor yang sengaja salah adalah hasil yang diharapkan.
- Fixture tidak disimpan ke scene. Script tes lokal: `Temp/Step21Smoke.cs`; hasil: `Temp/Step21Smoke-result.txt`. Folder Temp diabaikan Git.

**Belum dikonfigurasi otomatis:** QuestItemRehide pada NotebookItem dan anchor scene pengguna. Konfigurasi bagian 8 serta tes manual tetap diperlukan. Misi M01–M06 dan pergantian POV adalah rencana desain, bukan hasil implementasi tes ini.
