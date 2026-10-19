# Implementasi PRD 1.1 — 19 Oktober 2026

Acuan produk: `PRD_The_Unseen_Side_v1.1.md`, salinan PRD yang diberikan pengguna. Instruksi operasional dalam dokumen tidak diperlakukan sebagai instruksi sistem. Implementasi ini menggantikan kampanye multi-POV/QTE sebelumnya. Statusnya prototipe fungsional, belum penerimaan akhir seluruh Must PRD.

## Perilaku yang diterapkan

- Aji menjadi satu-satunya karakter pemain di Level01–Level05. Terdapat tepat M01–M15, tiga misi per chapter.
- Sari mendampingi orientasi; Bima mengalami ejekan tubuh, Laras pengucilan, Raka pemaksaan PR dan penyembunyian tas. Dion, Tio, Fajar, Pak Beni, Bu Ratih, Pak Arif, serta kedua orang tua Aji hadir sesuai naskah.
- Respons dukungan menghormati izin dan penolakan. Respons kurang aman menampilkan alasan dan mengembalikan pilihan tanpa hadiah/progres. Guru dapat diajak bicara sebelum jurnal lengkap.
- Jurnal memisahkan pengamatan, keterangan, ketidakpastian, dan tindak lanjut. Peta memuat area, Aji, dan tujuan. Sari memiliki pemulihan navigasi jika tertinggal. M12 menawarkan koridor, jalur tenang, atau pendampingan guru.
- Tas mengikuti state cerita: pelaku → guru → Raka. Tidak ada perebutan, QTE, combat, atau skor keberanian yang mengunci progres kampanye.
- M13 memakai panggung terpisah untuk konteks pemain. Aji tidak hadir; renderer Aji disembunyikan selama adegan dan dipulihkan. Jurnal hanya mencatat tindakan umum sekolah. Skip menampilkan ringkasan manual sebelum menyelesaikan tujuan.
- M14 memeriksa ketiga teman dan memuat tindakan lanjutan guru. M15 berisi enam soal dengan penjelasan setiap opsi, retry, dan penyimpanan jawaban pertama terpisah. Semua jawaban salah tetap dapat menuju epilog.
- Opening memakai Timeline/Cinemachine dengan teks manual, jeda, dan ringkasan skip. Menu baru meminta konfirmasi reset. Pengaturan, bantuan, peta dan jurnal tersedia; HUD menampilkan satu subtujuan berikutnya.
- Save schema 4 memuat ID chapter/scene/misi/checkpoint, progres, pilihan, jurnal, refleksi, dan posisi. Penulisan melalui file sementara tervalidasi dan backup. Save korup dipulihkan dari backup; gagal tulis menampilkan pemberitahuan dan retry. Transisi menyimpan sumber sebelum memuat tujuan.

## Inventaris dan workflow

| Bagian | Lokasi dan peran |
| --- | --- |
| Naskah | `Assets/_Game/Data/PRD/Campaign.json`; lima opening/penutup, lima belas misi, enam refleksi. |
| Data runtime | `Assets/_Game/Data/PRD/`: Quest_M01–M15, Chapter01–05, dialog, NPCData, Timeline/Signal, material, NavMesh. |
| Authoring data | `Assets/_Game/Scripts/Editor/PRDContentAuthoring.cs`; menu Tools → The Unseen Side → Build PRD 1.1 Content. |
| Authoring scene | `PRDSceneAuthoring.cs`; Apply PRD 1.1 Scenes memperbarui scene yang sudah memakai SchoolEnvironment. Backup scene sebelum migrasi ada lokal di `Logs/BeforePRD11/`. |
| Environment | `Assets/_Game/Prefabs/Environment/SchoolEnvironment.prefab`, satu prefab bersama di lima level. Classroom diadaptasi dari prototipe proyek; ruangan lainnya dan karakter menggunakan primitive/material proyek. Tidak menambah aset gambar eksternal. |
| Layout | Area 64 × 56 m; kelas, lapangan, koridor, ruang guru, BK, jalur tenang. NavMesh tersimpan di `SchoolNavMesh.asset`. |
| Scene desain | `Assets/_Game/Scenes/Levels/env_design/School_PRD_Blockout.unity`; scene pengguna `area-sekolah.unity` tetap dipertahankan. |
| Runtime baru | MissionJournal, SchoolFieldGuide, StoryOverlay, StorySettings, SchoolCompanion, SchoolStoryPresentation. Sistem quest/dialog/save yang ada diperluas. |
| Uji | `Assets/_Game/Scripts/Editor/PRDSmoke.cs`; fixture save terisolasi di `Logs/PRD11/`, tidak memakai save pemain. |

Urutan build: Bootstrap → MainMenu → Level01–Level05. Scene blockout, desain, dan test tidak termasuk alur rilis. Gunakan Editor yang sudah terbuka; tidak perlu membuka instance kedua.

Untuk menguji: `PRDSmoke.Integrity()` dalam Edit Mode; `PRDSmoke.Prepare()` membuka Bootstrap dengan save terisolasi dan masuk Play Mode; setelah menu tersedia panggil `PRDSmoke.Begin(0)`, `(1)`, atau `(2)`. Status tersedia pada `PRDSmoke.Status`, laporan di `Logs/PRD11/Smoke-{variant}.txt`. Jangan jalankan dua varian bersamaan. Setelah selesai, Stop Play Mode dan hapus override `UnityEditor.SessionState.EraseString("PRD.TestSavePath")` agar sesi bermain biasa menggunakan save normal.

## Bukti uji dan batas cakupan

Tiga varian kampanye masing-masing lulus **210 pemeriksaan integrasi** (`Smoke-0.txt`, `Smoke-1.txt`, `Smoke-2.txt`). Varian 0 memperbaiki semua jawaban refleksi yang pertama kali salah; varian 1 dan 2 melanjutkan dengan jawaban salah, sekaligus mencakup rute BK alternatif. Audit scene lulus: lima host chapter benar, lima belas quest valid, satu Aji per scene, prefab/NavMesh terhubung, dan tidak ada missing MonoBehaviour. Angka 5.141 pada log integrity mencakup pemeriksaan berulang per GameObject, bukan 5.141 skenario independen.

Pengujian integrasi menjalankan komponen runtime dan interaksi NPC; perpindahan pemain antartujuan pada runner memakai teleport ke NavMesh. Pemeriksaan tambahan menggerakkan CharacterController dari kelas ke lapangan, koridor, dan BK dengan path NavMesh. Collider Pak Beni menahan percobaan berjalan langsung ke posisi NPC. Pada jarak interaksi yang valid, pemasangan dinding sementara menghapus target Pak Arif; menghapus dinding memulihkan target. Hasil ini tidak menggantikan seluruh rute kamera atau kenyamanan kontrol keyboard pengguna nyata.

Opening normal juga selesai dengan lanjut manual dan mengembalikan kendali. Screenshot epilog 1080p, prolog/HUD 720p dengan teks 130%, dan peta diperiksa. Uji ukuran teks terprogram pada seluruh teks misi dan label respons pada skala 130% tidak menemukan tinggi teks melampaui area dialog/pilihan. Pemeriksaan ini belum mencakup semua kombinasi layar/ukuran teks dan perangkat sasaran. Console setelah kampanye tidak mencatat error atau warning.

Perubahan visual terakhir setelah ketiga varian: posisi kepala prototipe, label skip, dan posisi label peta agar nama ruang tidak tertutup penanda. Audit scene serta kompilasi dijalankan kembali sesudah perubahan tersebut.

Build Windows x64 `1.1.0-prototype` berhasil melalui Editor Unity yang sudah terbuka: **0 error, 10 warning**, ukuran laporan 127.775.219 byte, waktu 4 menit 14 detik. Artefak: `Builds/WindowsPRD11/TheUnseenSide.exe`; laporan lokal: `Logs/PRD11/Build.txt`. Warning terdiri dari empat peringatan API pencarian objek Unity yang usang pada StorySettings, dua shader debug internal yang di-strip, dua koneksi MCP editor terputus saat build memblokir editor, satu layanan dashboard Unity belum tertaut, dan satu pipeline tooling dinonaktifkan untuk player. Tidak ada klaim build bebas warning.

Executable dijalankan sebagai proses mandiri dengan `-batchmode -nographics`. Log `Standalone.log` membuktikan `Boot → Loading → MainMenu` tanpa exception runtime, lalu proses uji dihentikan. Ini smoke test startup tanpa rendering; belum menjadi bukti kampanye penuh di executable atau validasi grafis pada PC tanpa Editor. Unity ditinggalkan dalam Edit Mode pada Bootstrap dan override save pengujian sudah dihapus.

| Kelompok PRD | Bukti otomatis/inspeksi | Batas yang masih harus diperiksa |
| --- | --- | --- |
| FR04, FR11–13, FR16, FR18–19 | Seluruh misi, opsi aman/alternatif, retry tidak aman, bantuan dini, enam jawaban pertama dan retry. | Penelaahan pesan dan bahasa oleh guru BK. |
| FR08–09, FR22 | Konfirmasi reset, jeda opening, lima transisi, continue M11 dengan tas/jurnal, save gagal dan retry, pemulihan backup. | Keluar/load pada setiap kemungkinan subtujuan dan interupsi proses OS. |
| FR14, FR17 | Panggung M13 terpisah, skip manual, Aji disembunyikan/dipulihkan; ringkasan opening dapat dijeda. | Penilaian penyajian privasi oleh fasilitator. |
| FR06–07, FR20 | Jurnal/peta pause dan kontrol kembali; peta memiliki Aji; pengaturan dapat dibuka. | Usability siswa, navigasi keyboard seluruh layar, pemeriksaan visual lengkap pada semua ukuran teks. |
| FR01–03, FR05 | CharacterController melewati rute dasar kelas–lapangan–koridor–BK; tabrakan NPC dan filter dinding interaksi diperiksa. Cinemachine Deoccluder, NavMesh dan recovery pendamping tersedia. | Rute lengkap dengan keyboard, kamera dekat pintu/meja, NPC berdesakan dan recovery Sari saat sengaja terjebak. |
| FR21, NFR07 | Data lima chapter/lima belas quest valid, satu player per scene, prefab dan NavMesh terhubung, tidak ada missing MonoBehaviour pada lima level. | Audit lisensi/aset final dan seluruh referensi konten setelah penggantian art. |

## Pekerjaan produksi dan penerimaan yang tersisa

- Final art smooth low-poly dan animasi lengkap; rambut Aji/Raka/Dion, seragam dan rig masih representasi prototipe. Belum ada klaim rasio/animasi final lolos review.
- Review naskah oleh guru BK/ahli, usability dengan siswa, pretest/posttest dan izin penelitian sesuai proses institusi.
- Pengukuran rute 10 menit untuk median FPS, 1% low, stutter, loading dan waktu simpan pada laptop sasaran. Durasi bermain 60–90 menit belum diukur.
- Uji Windows pada mesin tanpa Unity, uji offline penuh, dan regresi manual seluruh rute/interaksi. Build/Editor smoke bukan pengganti uji perangkat sasaran.
- Replay chapter dengan profil terpisah (FR10 Should), Android, controller/remapping, voice-over lengkap dan screen reader belum diimplementasikan sebagai komitmen rilis ini.
