### 如果要使得某个数据可以存档，需要满足以下条件：
1. 数据需要实现 `IArchiveData` 接口。
2. 数据需要在 `ArchiveDataContainer` 中注册。

### 进行存档操作时，需要调用 `ArchiveManager` 中的 `SaveArchive` 方法。
```private void SaveArchiveInternal(ArchiveData archiveData);```
