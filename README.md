# unseen-side-bullying-game
Unseen Side merupakan game edukasi bullying yang dikembangkan untuk memenuhi Tugas Akhir dan syarat kelulusan dari Politeknik Negeri Semarang. Pengembangan menggunkan metode GDLC dan menggunakan game engine Unity.

Implementasi terbaru mengikuti [PRD 1.1, 19 Oktober 2026](document/PRD_The_Unseen_Side_v1.1.md): Aji sebagai satu karakter pemain, 5 chapter, 15 misi, dan 6 refleksi. Rincian perubahan, hasil uji, inventaris, serta pekerjaan rilis yang masih tersisa ada di [catatan implementasi](document/PRD11_Implementation.md). Dokumen multi-POV dan QTE sebelumnya merupakan riwayat desain.

Jalankan dari scene `Assets/_Game/Scenes/00_Bootstrap/00_Bootstrap.unity` pada Unity 6000.6.3f1. Build Windows tersedia secara lokal di `Builds/WindowsPRD11/TheUnseenSide.exe`; distribusikan seluruh folder build, bukan hanya berkas EXE. Folder build dan log tidak disimpan di Git.

Kontrol: WASD berjalan, Shift lari ringan, mouse mengatur kamera, E berinteraksi, J jurnal, M peta, Esc jeda/tutup panel. Enter atau tombol Lanjut membaca dialog; tombol arah dan Enter memilih respons/menu. Menu Pengaturan menyediakan volume, ukuran teks, sensitivitas kamera, pembalikan vertikal, dan teks langsung lengkap.

Checkpoint tersimpan setelah tujuan yang selesai dan pada akhir chapter. **Lanjutkan** memuat checkpoint terakhir; keluar di tengah percakapan mengulang percakapan tersebut. **Mulai baru** meminta konfirmasi. Jawaban refleksi pertama tetap tercatat walau pemain mencoba memperbaikinya; jawaban salah tidak mengunci ending.

Save lokal: `%USERPROFILE%\AppData\LocalLow\DefaultCompany\BullyingEducationGame\savegame_prd11.json`, dengan cadangan `.bak`. Save kampanye lama `savegame.json` tetap dipertahankan dan tidak digunakan untuk kampanye PRD 1.1. Pengaturan disimpan terpisah melalui Unity PlayerPrefs. Reset mengarsipkan save sebelumnya; tidak menghapus pengaturan.

Versi ini adalah prototipe fungsional. Karakter dan sekolah masih memakai aset blok/prototipe. Validasi isi oleh guru BK, playtest siswa, durasi 60–90 menit, dan pengukuran performa pada laptop sasaran belum dinyatakan selesai. Permainan inti tidak membutuhkan akun atau koneksi jaringan. Android dan replay chapter terpisah belum termasuk build ini.
