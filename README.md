# StreamStore
自動下載CSV
在操作介面上增加指定設備來源(IP/PORT)後, 設定想要下載資料的時間範圍, 即可自動下載該設備在這段時間範圍的歷史資訊(CSV檔)
# 注意事項:
  - 下載過程採"分段時間"下載, 可紀錄最後下載時間, 再自該時間, 繼續下載。
  - 軟體預設recevie time-out時間, 約20秒左右上下。
  - 由於設備於HTTP HEADER未附帶Content-Length相關提示封包內容大小資訊, 所以在分段時間下載的資訊, 仍有可能有部分時間的資訊是空白, 這是由於載到一半的資料, 軟體發出timeout的例外事件。
  - 可用xalelu/SimulationServer模擬設備回傳歷史資料
