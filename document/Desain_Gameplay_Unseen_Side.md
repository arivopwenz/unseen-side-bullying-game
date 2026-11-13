# Desain Gameplay Unseen Side — Chapter 1

> **Status:** Desain dari Ari, disampaikan 4 November 2026, untuk disimpan agar tidak hilang.
> **Sumber:** Pesan Ari tentang Chapter 1 (bukan usulan Muse).
> **Catatan penting:** Dokumen ini TIDAK menimpa GDD v2 (`GDD_Unseen_Side_v2_Kartu.md`).
> GDD v2 memakai struktur lama (15 misi). Dokumen ini mengikuti struktur baru yang
> diminta Ari: prolog + Chapter 1–4. Cerita Chapter 2–4 belum ada di dokumen ini
> dan akan ditambahkan setelah Ari menyampaikannya.

## 1. Struktur keseluruhan (rangka dari Ari)

- **Prolog:** cutscene kepindahan Aji / pemain utama.
- **Chapter 1–4:** tiap chapter diisi 2 objective.
- **Total:** sekitar 8 objective + prolog = sekitar 8–9 cutscene yang dibuat Ari.
- **Dokumen ini:** baru mencakup **Chapter 1** secara lengkap. Chapter 2–4 masih kosong,
  menunggu pembahasan Ari berikutnya.

## 2. Karakter dan lokasi Chapter 1

**Karakter yang terlibat di Chapter 1:**

- **Aji** — pemain utama, murid baru.
- **Bima** — korban gendut.
- **Radit** — korban kutu buku berkacamata.
- **Memet** — pembully utama.
- **Raka** — pembully kedua.
- **Bu Dyah** — yang memperkenalkan Aji di cutscene perkenalan kelas
  (lihat bagian 6: peran ini perlu dikunci, karena kanon sebelumnya mencatat
  Bu Dewi sebagai Wali Kelas dan Bu Dyah sebagai Guru BK perempuan yang baik).
- **Guru BK laki-laki bermuka garang** — berpatroli di fase pencarian
  (namanya tidak disebut di pesan Chapter 1 ini; kemungkinan Pak Toto dari kanon
  sebelumnya, BELUM dikunci).

**Lokasi Chapter 1:**

- Kelas.
- Kantin.
- Belakang kantin.
- Titik-titik di sekitar kantin (5 titik pencarian).

## 3. Alur Chapter 1 — Objective 1

1. Objective 1 sudah benar dari versi sebelumnya, dengan satu penegasan alur:
   setelah selesai dibully karena diejek postur Bima, mereka **langsung menuju ke Kelas**.
2. **Trigger cutscene perkenalan kelas:** Bu Dyah mengenalkan murid baru, yaitu Aji.
3. **Bel istirahat berbunyi.**
4. **Geng pembully melakukan pemalakan.**
5. Bima dan Radit **tidak punya uang** dan akhirnya **dipukul Raka**.
6. **Cutscene lagi:** Aji bertanya kenapa tadi Bima dipukul, dan seterusnya.
7. Bima **mengajak istirahat ke Kantin**.

## 4. Alur Chapter 1 — Objective 2

8. **Objective 2 muncul:** ke kantin bersama Bima.
9. **Di tengah jalan, trigger cutscene:** Bima membawa **botol minum dan bekal**.
   Tiba-tiba geng pembully datang, **merebut jajan Bima**, lalu pergi ke **belakang kantin**.
10. **Bima sedih (masih dalam cutscene)**, dan Aji **berinisiatif mencarikan**
    botol dan bekal Bima.
11. **Di sinilah fitur kartu digunakan.**

## 5. Fitur kartu di Objective 2

**Prinsip utama (dari Ari):**

- Pemain mendapat **Kartu Aksi ATAU Kartu Hati**.
- **Objective sama** untuk kedua kartu: tetap mencari botol dan bekal Bima.
- Yang beda hanya **cara penyampaiannya**.

### 5.1 Jalur Kartu Aksi (satu-satunya jalur yang dijabarkan detail)

1. Pemain **mencari ke belakang kantin**.
   - Di pesan Ari tertulis mencari "Raka dan Radit" di belakang kantin.
     Ini disimpan persis seperti itu, dan masuk daftar konfirmasi di bagian 6,
     karena Radit sejauh ini adalah korban kedua.
2. **Trigger mini cutscene Dialogue Decision:** pemain harus memilih dialogue decision.
3. **Pilihan salah memicu QTE** karena Memet marah murka.
   - Detail QTE (tombol, durasi, akibat gagal QTE) belum dijabarkan.
4. **Bar respect / hati si Memet:**
   - Visual bar naik **dari bawah ke atas**.
   - Jawaban benar: **+20**.
   - Jawaban salah: **−10**.
   - Bar penuh di angka **100**.
   - Muncul **5 pertanyaan atau lebih** seputar pembullyan.
   - Pertanyaan bersifat **informatif**, membantu pengetahuan pemain,
     dengan **bahasa yang mudah dipahami dan tidak kaku**.
   - Hitungannya: 5 jawaban benar pas 100 (5 × 20). Sekali salah, pemain butuh
     pertanyaan tambahan untuk menutup minus 10 itu — karena itu Ari menyebut
     "5 pertanyaan atau lebih", bukan tepat 5.
5. **Ketika bar sudah penuh:**
   - Muncul dialogue yang **memberi tahu letak botol minum dan bekal Bima**.
   - **Cutscene kamera** menunjukkan titik-titik di sekitar kantin tempat
     bekal dan botol disembunyikan.
   - **Bunyi alarm istirahat selesai.**
   - Ada **batas waktu** (durasi belum disebut).
   - **Penjaga Guru Bapak-bapak bermuka garang mulai berpatroli.**

### 5.2 Jalur Kartu Hati

- **Belum dijabarkan** di pesan Chapter 1 ini.
- Yang sudah pasti: objective-nya sama dengan jalur Aksi, yaitu mencari botol
  dan bekal Bima, hanya cara penyampaiannya yang beda.
- Bagian ini dibiarkan kosong dan TIDAK diisi dengan karangan, menunggu penjelasan Ari.

## 6. Fase pencarian: 5 titik di sekitar kantin

1. Pemain harus mencari **1 per 1 di 5 titik** sampai ketemu.
2. Tiap titik: **mencet tombol E dan menunggu 5 detik**,
   divisualisasikan dengan **UI loading World Space**.
3. Sistem ini **gacha**: lokasi botol dan bekal **acak** di antara 5 titik itu.
4. Ketika ketemu, muncul **UI notifikasi: botol dan bekal sudah ditemukan**.
5. Pemain **diarahkan kembali ke Bima tanpa ketahuan guru BK yang berpatroli**.
6. **Ketika Aji ketahuan:**
   - Trigger **cutscene Aji dipergoki guru BK**.
   - Mengulang dari **titik spawn awal mencari botol / reset progress**.
   - Ruang lingkup reset (hanya pencarian, atau termasuk bar dialogue)
     belum dikunci — lihat bagian 7.

## 7. Ringkasan angka dan aturan Chapter 1

| Elemen | Nilai / aturan (dari Ari) |
|---|---|
| Objective per chapter | 2 |
| Bar respect / hati Memet, penuh | 100 |
| Jawaban dialogue benar | +20 |
| Jawaban dialogue salah | −10 |
| Pertanyaan dialogue | 5 atau lebih, seputar pembullyan, informatif, bahasa mudah, tidak kaku |
| Pilihan dialogue salah | Memicu QTE (Memet marah murka) |
| Titik pencarian sekitar kantin | 5 titik, dicari 1 per 1 |
| Interaksi tiap titik | Tombol E, tunggu 5 detik, UI loading World Space |
| Penempatan botol dan bekal | Acak (gacha) |
| Ketemu | UI notifikasi botol dan bekal sudah ditemukan |
| Ketahuan guru BK berpatroli | Cutscene dipergoki, ulang dari spawn awal mencari botol / reset progress |
| Pemicu fase patroli | Bar Memet penuh + alarm istirahat selesai |

## 8. Yang belum dijabarkan / perlu dikunci nanti

Daftar ini sengaja TIDAK diisi dengan jawaban karangan. Semuanya menunggu
konfirmasi Ari, dan tidak menghalangi pembahasan Chapter 2:

1. **Jalur Kartu Hati** untuk Objective 2 Chapter 1.
2. **"Raka dan Radit" di belakang kantin** — apakah maksudnya Raka dan Memet
   (geng pembully), karena Radit sejauh ini korban kedua.
3. **Peran Bu Dyah di kelas** — apakah Bu Dyah double role, atau ada perubahan
   peran dari kanon sebelumnya (Wali Kelas Bu Dewi, Bu Dyah Guru BK perempuan baik).
4. **Detail QTE** — tombol, durasi, dan akibat gagal QTE selain pengurangan bar.
5. **Durasi batas waktu** di fase pencarian setelah alarm istirahat selesai.
6. **Bar Memet: lokal atau global?** — apakah bar ini hanya untuk misi ini saja,
   atau terakumulasi sampai akhir game.
7. **Ruang lingkup reset saat ketahuan BK** — hanya pencarian, atau termasuk
   bar dialogue.
8. **Hubungan bar Memet dengan bar respect korban untuk secret ending**
   (dari brainstorming sebelumnya: 2 korban = 2 secret ending, bar tidak penuh
   sampai chapter terakhir = secret bad ending) — belum dikunci apakah
   bar Memet ini bagian dari sistem itu atau sistem terpisah untuk misi ini.
9. **Istilah Kartu Hati vs Kartu Kata** — di GDD v2, Hati sudah diganti Kata
   atas pilihan Ari (2 November 2026), tetapi di desain Chapter 1 ini Ari memakai
   istilah Kartu Hati lagi. Perlu dikunci apakah Hati kembali, atau Hati = Kata.
10. **Chapter 2–4** — belum dibahas, dokumen ini akan ditambah setelah Ari
    menyampaikannya.

## 9. Riwayat dokumen

- **4 November 2026:** Ari menyampaikan desain Chapter 1 dan meminta telaah
  mendalam plus penyimpanan di memori.
- **4 November 2026:** File ini dibuat atas permintaan Ari ("buatkan file .md
  dari gameplay ini, semuanya, buat memori ke depannya biar ga ilang"),
  berdasarkan pesan Chapter 1 tersebut dan rangka prolog + Chapter 1–4
  (2 objective per chapter) yang sudah ia tetapkan sebelumnya.
- File ini adalah **catatan desain**, bukan implementasi di Unity. Tidak ada
  script, scene, atau file game yang diubah karena pembuatan dokumen ini.
