# Panduan Level01: zona, cutscene asli, dan gerakan

Versi ini menggantikan alur prototipe terpisah yang dibuat pada implementasi awal. Hierarki asli menjadi penggerak utama: `Bima_Guide`, `GuideRoute`, `Nakal`, `Nakal1`, `AmbushGang`, `AmbushTriggerZone`, `WalkTalkCanvas`, `Canteen Cutscene Trigger`, dan `BimaConversationDriver`. Komponen `LevelOneExistingFlow` di GameObject **Script** menghubungkan kejadian asli dengan progres PRD, Kartu Kata, pencarian barang, dan jurnal.

## Alur yang dipakai

1. Bima memakai **GuideController** dan waypoint **GuideRoute** seperti semula. Subtitle perjalanan memakai **WalkTalkDriver**.
2. Memasuki **AmbushTriggerZone** menjalankan **AmbushTrigger → AmbushGang → AmbushDialogueDriver** beserta kamera aslinya. Sesudah kejadian selesai, Kartu Kata K-01 terbuka. Bima melanjutkan perjalanan setelah puzzle selesai.
3. Memasuki **GuideRoute/Zone_PerkenalanKelas** mengaktifkan **YellowCircleTrigger** yang sudah ada. Referensi **Activation Zone** pada komponen itu menunjuk BoxCollider zona kelas. Cutscene dimainkan di scene **Cutscene perkenalan kelas** dengan Timeline **MasukKelas**. Setelah selesai, pemain kembali ke Level01 untuk memeriksa kursi Sikka.
4. Percakapan dan perjalanan ke kantin tetap memakai **BimaConversationDriver**. Tujuannya **GuideRoute/BimaMark_Kantin**.
5. Memasuki **Canteen Cutscene Trigger** mengaktifkan **BekalCutsceneTrigger**. Scene **BekalCutscene**, GameObject **Timeline Cutscene/Timeline Canteen**, memakai Timeline **Canteen** yang sudah ada.
6. Setelah kembali, lanjutkan K-02, Fatimah, pencarian P1–P5, izin petugas, pengembalian barang, K-03, dan catatan akhir. Pemeran Bima yang dipakai adalah **Bima_Guide** asli. Salinan Bima/Memet/Raka/Bu Ratna serta kamera adegan prototipe dinonaktifkan.

Kedua cutscene harus selesai sebelum progres adegannya dicatat. Zona berikutnya tidak aktif sebelum syarat cerita dipenuhi. Jika permainan terputus saat cutscene, pemain dapat mengulang dari zona; adegan tidak dianggap selesai begitu saja.

## Mengubah posisi dan ukuran zona

Lakukan dalam **Edit Mode**, kemudian simpan scene. Perubahan posisi sewaktu Play tidak menjadi perubahan permanen.

| Yang diubah | GameObject / Inspector |
| --- | --- |
| Lokasi konfrontasi | **AmbushTriggerZone → Transform**. |
| Luas zona konfrontasi | **BoxCollider → Edit Collider / Center / Size**; pertahankan **Is Trigger**. |
| Tempat pembully berhenti | **CP_Nakal** dan **CP_Nakal1**; referensinya ada di **AmbushGang → Members → Confrontation Point**. |
| Rute Bima | Pindahkan waypoint **GuideRoute/WP_…**, lalu periksa urutan **Bima_Guide → GuideController → Waypoints**. |
| Zona perkenalan kelas | **GuideRoute/Zone_PerkenalanKelas → Transform dan BoxCollider**. Ini ditempatkan berdasarkan marker kelas yang sebelumnya sudah ada; pindahkan ke pintu VIII A yang kamu inginkan. |
| Zona kantin | **Canteen Cutscene Trigger → Transform dan BoxCollider**. |
| Tujuan jalan Bima ke kantin | **GuideRoute/BimaMark_Kantin**. Pindahkan bersama zona kantin agar Bima berhenti dekat zona itu. |
| Titik pencarian / pengembalian | **[Level01 PRD - Langkah Pertama]/01 - Titik Interaksi… → P1–P5, Staff, Fatimah, Bench**. |

Jangan mengubah ID `LevelOnePoint`. Penanda awal ClassFront/Corridor/Teacher/Canteen dari prototipe bukan pemicu cutscene dalam mode integrasi ini. **Zona asli yang memicu**, tanpa menekan E. E tetap dipakai untuk inspeksi, berbicara, dan mengambil/mengembalikan barang.

Pastikan semua titik berdiri di NavMesh. Beri jarak antartitik pembully minimal sekitar 1,5–2 meter, dan jangan menempatkannya tepat di tubuh Bima. Jika waypoint dipindahkan melintasi dinding atau tangga baru, periksa jalur NavMesh; memindahkan marker tidak otomatis membangun ulang NavMesh.

## Membuka Timeline yang benar

**Perkenalan kelas**

- Buka scene `Assets/_Game/Scenes/Levels/Level_01/Cutscene perkenalan kelas.unity`.
- Pilih GameObject **Cinemachine** yang mempunyai **Playable Director**.
- Buka **Window → Sequencing → Timeline**. Asset yang dipakai adalah `Assets/_Game/Cutscene/Scene Masuk kelas/MasukKelas.playable`.
- Contoh binding: **Animation Track → karakter_utama**, **Animation Track (1) → IbuGuru@Idle**, **Animation Track (2) → gendut@Old Man Idle (3)**. Track kamera terhubung ke **Main Camera**.

**Kantin**

- Buka scene `Assets/_Game/Scenes/Levels/Level_01/BekalCutscene.unity`.
- Pilih **Timeline Cutscene/Timeline Canteen** dan buka Timeline. Pastikan Playable Director menunjuk asset **Canteen**.
- Binding karakter memakai **karakter_utama**, **Bima_Guide**, **Nakal**, dan **Nakal1** di scene cutscene tersebut; jangan menggantinya dengan objek dari Level01 yang sedang tidak dimuat.

Jangan mengedit `MasukKelas_Level01` atau `Opening01` dengan dugaan itu cutscene yang sedang dimainkan. Alur zona kelas ini memakai **MasukKelas** di scene khususnya.

## Memperbaiki gerakan sendiri

1. Simpan salinan scene dan Timeline sebelum eksperimen. Periksa bagian pendek dengan menggeser playhead; matikan mode Record sesudah selesai.
2. Untuk mengubah **kapan** karakter bergerak, geser klip animasi pada track yang terikat ke karakter itu. Untuk mempercepat atau memperlambat satu klip, pilih klip dan atur **Speed Multiplier** pada Inspector. Jangan sekadar memperpanjang animasi berjalan bila karakternya sudah berhenti.
3. Untuk menghaluskan sambungan, tumpangtindihkan ujung dua klip sedikit agar terjadi blend. Mulai dengan sekitar **0,15–0,3 detik** antara idle dan berjalan, lalu lihat kaki serta tubuh pada transisinya. Hindari overlap terlalu panjang karena pose dan gerakan bisa terlihat mengambang.
4. Jika karakter meloncat posisi saat klip berganti, periksa **Track Offsets**, posisi/rotasi awal track, dan offset klip. Selaraskan akhir klip pertama dengan awal klip berikutnya. Jangan menambal loncatan dengan memindahkan semua tulang karakter.
5. Jika ingin merekam perubahan posisi/rotasi, gunakan klip rekaman `.anim` yang bisa diedit dan tombol Record pada track yang benar. Periksa binding terlebih dahulu. Klip animasi yang berasal dari FBX dapat bersifat read-only; buat salinan/klip rekaman sendiri sebelum mengubah kurvanya.
6. Untuk gerak dari A ke B yang kamu animasikan sendiri, gunakan kurva posisi dengan beberapa keyframe yang jelas: mulai, mendekat, berhenti. Kurva lurus cocok untuk kecepatan tetap; kurva Ease In/Out membantu awal dan akhir gerakan. Sinkronkan perpindahan dengan langkah animasi agar kaki tidak meluncur.
7. Saat Timeline mengendalikan karakter, **PlayerMovement**, **PlayerInputHandler**, **PrototypeCharacterMotion**, dan **NavMeshAgent** karakter itu tidak boleh ikut menggerakkannya. Pada kedua scene cutscene, pengendali gameplay yang berbenturan sudah dinonaktifkan; jangan menyalakannya kembali untuk membuat karakter berjalan di Timeline.
8. Untuk kamera, pilih klip di **Cinemachine Track**, lalu pilih kamera virtual yang dirujuk klip tersebut. Ubah posisi, arah, dan lensanya pada kamera virtual. Atur overlap shot untuk transisi halus. Jangan merekam Transform Main Camera karena posisinya dikendalikan Cinemachine Brain.

Contoh nyata pada **MasukKelas**: Aji mempunyai klip **Walking** sekitar detik **22,8–26,75**. Mulai latihan dengan memperbaiki blend dari **Breathing Idle → Walking → Breathing Idle** pada bagian itu, sebelum mengubah seluruh adegan.

## Gerakan konfrontasi awal

Konfrontasi di **AmbushTriggerZone** menggunakan script, bukan Timeline baru. Untuk rute dan tempo dasarnya:

- **Bima_Guide → GuideController**: Walk Speed, Wait Distance, Resume Distance, dan urutan waypoint.
- **Nakal / Nakal1 → NavMeshAgent**: Speed, Acceleration, Angular Speed, dan Stopping Distance.
- **AmbushGang → BullyGroupController**: formasi Members, Look Target, Approach Timeout, dan Return Timeout.
- **AmbushDialogueDriver**: teks dan durasi dialog; Wait For Key Advance menentukan apakah menunggu input.
- **AmbushTriggerZone → AmbushTrigger**: kamera, guide, group, dan durasi beat. Dorongan masih berupa gerak script dalam `HitBeat()`; untuk menggantinya dengan animasi rig, buat klip aksi yang sesuai dan sinkronkan reaksi Bima. Jangan mengaktifkan root motion dan perpindahan script untuk gerakan yang sama sekaligus.

## Yang harus dipertahankan agar kembali ke gameplay

Pada Playable Director gunakan **Wrap Mode: None**. Pertahankan **CutsceneAutoReturn** aktif dan **Authored Director** mengarah ke Director yang benar. `Timeout Seconds` adalah pengaman, bukan durasi yang disarankan untuk adegan; bila Timeline diperpanjang melampaui batasnya, naikkan pengaman itu. Jangan mengaktifkan Play On Awake pada Director lain dalam scene.

Referensi prefab lingkungan lama **School_Environtment** dan **School_Environtment2** masih hilang pada scene cutscene. Ini sudah ada sebelum penyesuaian. Untuk memperbaiki latar, pulihkan prefab beserta file `.meta` aslinya dari backup, atau pasang prefab lingkungan yang kamu pilih lalu sesuaikan panggung dan kamera terhadapnya. Binding aktor/Timeline tidak diganti dengan cutscene baru.

Checkpoint PRD memakai berkas terpisah dari save kampanye lama. Ulangi demo melalui **ESC → Ulang Level 1**. Laporan pengujian alur asli ada di `Logs/LevelOnePRD/ExistingFlow-Smoke.txt`; hasil uji prototipe sebelumnya tidak menggantikan pengujian integrasi zona ini.

Status verifikasi saat penyesuaian: konfrontasi asli, pembukaan K-01, dan pemuatan asset MasukKelas sudah teramati. Pengujian pergi–pulang kedua scene belum lulus penuh: percobaan runner berhenti karena batas waktu pemuatan/Editor, disusul putusnya koneksi MCP saat reload assembly. Assembly runtime dan Editor terbaru telah dihasilkan setelah perubahan sumber. Jangan menganggap hasil 143 assertion versi awal sebagai bukti bahwa transisi baru sudah lolos. Runner memakai save terisolasi dan pencarian posisi otomatis; versi runner terbaru juga melakukan seek ke detik akhir untuk memeriksa penyelesaian Timeline, sehingga bukan review pemutaran visual sepanjang adegan.
