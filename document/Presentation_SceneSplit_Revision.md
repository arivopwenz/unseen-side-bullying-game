# Revisi tampilan, cinematic dan scene — 7 Oktober 2026

Dokumen ini menggantikan informasi tampilan/routing satu-scene pada `Chapter01_02_Narrative_Revision.md`. Acuan terbaru: tampilan bersih, quest di kanan, perkenalan karakter lewat Timeline, dan satu chapter per scene. Pengecualian kelas indoor untuk Chapter 2 tetap berlaku.

## Tampilan

| Fase | UI yang tampil |
|---|---|
| Gameplay | Panel QUEST di kanan, objective belum selesai, panah/jarak; prompt interaksi ketika dekat |
| Percakapan | Dialogue, Lanjut, pilihan Aji jika tersedia |
| Pembukaan Timeline | Dialogue dan Lewati pembukaan di kanan atas panel dialogue |
| Konfrontasi / aftermath | Dialogue; efek tekanan jika pengaturan mengizinkannya |
| QTE | Usaha/timer; quest dan tulisan petunjuk tambahan tersembunyi |
| Pertanyaan quest | Kartu pertanyaan, lalu penjelasan dan Lanjut |
| Hasil chapter | Penutup dan tombol chapter berikutnya |

Header Chapter/POV, nama model yang mengambang, petunjuk panjang di bawah layar, dan teks berulang "Beli makanan / bicara — Bu Sari • jarak • depan" disembunyikan. Kegiatan kantin tetap ada untuk menjaga cerita; objective menjadi "Datangi kantin saat istirahat".

Panel quest selebar 440 pada reference resolution 1920 × 1080; tinggi mengikuti teks. Objective selesai dan hitungan 0/1 untuk tugas satu kali dihilangkan. Panah menunjuk sudut jalur NavMesh berikutnya relatif terhadap kamera, sedangkan jarak menunjukkan jarak lurus ke tujuan. Panah menghilang ketika dekat tujuan.

Kartu pertanyaan menjadi 880 × 500. Penjelasan menggantikan isi pertanyaan, sehingga kedua tampilan tidak menumpuk. Panel matematika menjadi 880 × 360. Respons Aji diselaraskan dengan lebar dialogue. Label quest menjadi "Memahami kejadian Denis" / "Memahami tekanan Denis"; ID internal dan checkpoint dipertahankan.

Pertanyaan "Kalah pada QTE berarti…" diganti dengan tanggung jawab kelompok Billy atas ancaman dan upaya merebut buku. Pertanyaan berlaku pada kedua hasil encounter. Keberhasilan input tidak diperlakukan sebagai ukuran nilai diri korban.

## Routing dan checkpoint

| Scene | Chapter / karakter |
|---|---|
| Level01 | 1 — Aji |
| Level02 | 2 — Denis |
| Level03 | 3 — Billy |
| Level04 | 4 — Denis |
| Level05 | 5 — Ari |

Build Settings: Bootstrap, MainMenu, lima level. `NarrativeChapterData.SceneName` menentukan tujuan; `StoryChapterSequence.sceneChapterNumber` mengikat scene pada satu chapter. Katalog lima chapter dipakai untuk validasi/restore seluruh campaign, bukan memainkan semuanya dalam Level01.

Tombol lanjut menyimpan checkpoint asal, mengganti chapter/POV tersimpan, menghapus posisi player scene asal, lalu memuat scene berikutnya secara async. Lanjutkan pada menu memuat scene chapter tersimpan. Save lama dengan prasyarat belum lengkap diarahkan ke scene chapter pertama yang masih perlu diselesaikan. Catatan item scene sebelumnya digabung ke checkpoint, bukan dihapus saat scene baru tidak mempunyai item aktif.

Scene baru memakai salinan native kampus dan set lingkungan chapter yang sudah tersedia. Ini memberi file scene terpisah untuk pengembangan; belum merupakan lima environment final yang sepenuhnya berbeda. Semua authoring scene melalui Editor, tanpa edit YAML mentah.

## Cinematic

Setiap scene memiliki `TL_Level0N_Opening.playable`, Cinemachine Track dan Signal Track dengan binding lokal. Chapter 1 memakai establishing, shot Aji, Denis, Billy, Bima, Fajar, Ari, Pak Bambang, Bu Sari, dan closing. Signal menahan Timeline untuk dialogue; baris pembicara memilih shot miliknya. Penutup/skip memulihkan kontrol, cursor dan posisi aktor.

Matematika memakai area depan papan tulis: guru di kiri, gang di kanan, meja menyediakan ruang adegan, kamera tetap di dalam kelas. Gang menghadap Denis. Pemalakan makanan dimulai setelah percakapan Bu Sari, dengan Denis dan tiga anggota kelompok dalam shot depan kantin. Narasi teknis tentang QTE/pengaturan di percakapan kantin dihapus.

`NarrativeActivityDirector` memisahkan konfrontasi, QTE dan aftermath. Cancel tidak menyelesaikan objective; makanan, posisi gang, prioritas kamera dan kontrol dipulihkan. Bantuan QTE mempertahankan konfrontasi dan aftermath.

Kamera percakapan memakai anchor NPC atau sisi alternatif saat terhalang. `Cameras/DialogueFocus` sudah disiapkan; script menempatkannya di tengah dua karakter dan memakai Look At tersebut. Deoccluder belum ditambahkan otomatis, sesuai permintaan untuk mempelajarinya setelah pekerjaan utama.

## Manual: Deoccluder kamera percakapan

1. Keluar dari Play Mode. Buka Level01, pilih **Cameras → DialogueCamera**, yang memiliki Cinemachine Camera dan Dialogue Camera Framing. Jangan pilih Camera utama yang memiliki Cinemachine Brain.
2. Pastikan Look At / Custom Look At mengarah ke **Cameras/DialogueFocus**. Reference sudah dipasang; script menggerakkannya ketika percakapan mulai. Konfigurasi position/rotation script dapat dipertahankan.
3. Tambahkan **Cinemachine Deoccluder** melalui Add Extension atau Add Component. Extension memindahkan kamera saat collider menghalangi garis pandang menuju target.
4. Collide Against: hanya layer lingkungan dengan collider. Graybox sekarang memakai Default. Jika collider karakter juga memakai Default, buat layer **Characters** lewat Inspector → Layer → Add Layer, lalu pindahkan Player dan NPC beserta anak model/collidernya ke Characters. Jangan pindahkan lingkungan. Pada Deoccluder, centang Default/layer lingkungan dan keluarkan Characters serta Ignore Raycast. Dengan begitu badan lawan bicara tidak dianggap tembok. Ignore Tag: Player. Periksa interaksi/trigger setelah perubahan layer; jangan mengubah matriks Physics collision tanpa kebutuhan.
5. Titik awal tuning: Avoid Obstacles ON; Camera Radius **0.2**; Minimum Distance From Target **0.5**; Distance Limit **0**; Strategy **Preserve Camera Distance**; Minimum Occlusion Time **0.05**; Smoothing Time **0.15**; Damping **0.4**; Damping When Occluded **0.15**; Maximum Effort **4**. Nilai ini belum merupakan hasil final playtest extension.
6. Play dari Bootstrap. Uji percakapan dekat dinding dan dari beberapa arah. Jika bergoyang, naikkan smoothing/damping sedikit. Jika terlalu mendekat, coba Preserve Camera Height dan periksa posisi anchor NPC.
7. Jika sesuai, salin component Deoccluder ke DialogueCamera di Level02–Level05 dan simpan setiap scene. DialogueFocus harus memakai objek dari scene masing-masing.

Deoccluder memerlukan collider penghalang. Ia menjaga satu target; komposisi seluruh kelompok tetap memerlukan blocking dan group shot. Referensi: dokumentasi package terpasang `Library/PackageCache/com.unity.cinemachine@f53aa49b3acc/Documentation~/CinemachineDeoccluder.md`.

## Verifikasi

Authoring: `Tools/UnityAutomation/CleanChapterAuthoring.cs`. Test: `CleanPresentationSmoke.cs` dan `Chapter12Smoke.cs`, diperbarui untuk perpindahan scene. Fixture memakai save terpisah di Logs.

Pemeriksaan awal menemukan NavMeshPath kosong pada komponen yang diperbarui; diperbaiki dengan inisialisasi Awake dan fallback. Panel quest dipindahkan dari parent HUD lama ke SafeArea agar benar berada di kanan. Fixture reload scene diperbaiki agar menunggu instance sequence baru, bukan opening lama.

Pemeriksaan akhir pada Editor yang sama, 7 Oktober 2026:

| Pemeriksaan | Hasil |
|---|---|
| Integritas lima scene, binding Timeline lokal dan komponen | PASS — 93 pemeriksaan |
| Pembukaan melalui Signal, shot karakter, skip, HUD dan panah | PASS — 36 pemeriksaan |
| Jalur dukungan Chapter 1 | PASS — 32 pemeriksaan |
| Tombol pertanyaan/penjelasan nyata dan perpindahan Level01 → Level02 | PASS — 41 pemeriksaan |
| Denis: matematika benar dan input QTE berhasil | PASS — 64 pemeriksaan |
| Denis: matematika salah dan QTE kehabisan waktu | PASS — 64 pemeriksaan |
| Chapter 3–5, pilihan, hasil akhir dan reload checkpoint | PASS — 57 pemeriksaan |
| Tombol Lanjutkan nyata serta migrasi checkpoint lama | PASS — 6 pemeriksaan |
| Pause, cancel, retry, objective, bantuan QTE dan preferensi aksesibilitas | PASS — 44 pemeriksaan |

Console setelah pengujian: **0 error, 0 warning, compilationFailed false**. Pengaturan Play Mode sementara dikembalikan dan Editor dibuka pada Bootstrap dalam Edit Mode. Fixture tidak memakai file save pemain.

Screenshot hasil pengujian tersedia di `Logs/CleanPresentation/` dan `Logs/AutomationChapter12/`: `CleanHUD.png`, `Intro_Billy.png`, `QuestQuestion_2.png`, `ClassroomBullying_True.png`, dan `FoodConfrontation.png`. Panel diperiksa melalui koordinat layar; shot kelompok diperiksa agar Denis dan ketiga pembully masuk frame serta tidak tertutup panel dialogue.

Build Windows **Succeeded**, 0 error dan 3 warning, sekitar 151 detik / 126.8 MB. Output: `Builds/WindowsPrototype/TheUnseenSide.exe`; seluruh folder build harus tetap bersama executable. Laporan: `Logs/CleanPresentationWindowsBuildReport.json`.

Warning yang tersisa: RuntimePipelineConfig sengaja tidak aktif pada build pemain, serta shader debug `Hidden/Core/DebugOccluder` dan `Hidden/Core/DebugOcclusionTest` di-strip karena pipeline tidak sesuai. Console setelah build: 0 error, 3 warning build tersebut. Startup executable dengan `-batchmode -nographics` mencapai MainMenu dan tidak menemukan exception yang diperiksa; ini pemeriksaan startup, bukan verifikasi grafis build. Log: `Logs/CleanPresentationPlayerStartup.log`.

Pengujian otomatis dan screenshot memvalidasi kasus di atas, bukan jaminan bahwa seluruh kemungkinan interaksi sudah tercakup.

## Cara mencoba

Play dari **00_Bootstrap → Mulai baru** untuk melihat pembukaan/UI baru. Mulai baru mengganti checkpoint sebelumnya; Lanjutkan mempertahankan progress. Setelah menyelesaikan chapter, tombol lanjut harus mengubah nama scene di Editor. Model/animasi masih graybox sampai aset final pengguna dipasang; playtest manusia tetap diperlukan untuk tempo, keterbacaan dan pemahaman cerita.
