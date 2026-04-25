# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="1.3.0"></a>
## [1.3.0](https://www.github.com/mu88/PasswordTrainer/releases/tag/1.3.0) (2026-04-25)

### ✨ Features

* add test suite, security hardening, and CI quality gates ([3d028c1](https://www.github.com/mu88/PasswordTrainer/commit/3d028c1959ebb14bd4f72324605531260d7b2f22))
* migrate code coverage from coverlet to dotnet-coverage ([930d538](https://www.github.com/mu88/PasswordTrainer/commit/930d538de308ed1964e2a173566bcc36d9b72835))

### 🐛 Bug Fixes

* **ci:** enable Stryker dashboard reporting ([c26809d](https://www.github.com/mu88/PasswordTrainer/commit/c26809da3b065c2d0737e9425d002158ab995d1e))

### ♻️ Refactors

* **security:** replace DataProtection with AES-256-GCM + HKDF from pepper ([21e306a](https://www.github.com/mu88/PasswordTrainer/commit/21e306a1563cdc11cd190d2760c1fc81cfebd0b4))

### ✅ Tests

* improve coverage ([847b200](https://www.github.com/mu88/PasswordTrainer/commit/847b200a3d2f667b6f0bcdde8c7542a7b672f2b1))
* improve system test ([3eb845d](https://www.github.com/mu88/PasswordTrainer/commit/3eb845d4ef1f406c4568ce10f1a514165b8d77cb))
* simplify build process by using CliWrap ([0da68a2](https://www.github.com/mu88/PasswordTrainer/commit/0da68a2bc92ac3c897d116f3022d43b3048fac37))

### 🔧 Chores

* apply new code style ([7603d5a](https://www.github.com/mu88/PasswordTrainer/commit/7603d5a35d0d6ac70868041d09a9ae064f80f6b9))
* remove obsolete .stryker-config.json (renamed to stryker-config.json) ([8943f9b](https://www.github.com/mu88/PasswordTrainer/commit/8943f9bb7eede7546453649f812164bf8924afdc))
* use new shared GitHub repo mu88/common ([f855ad2](https://www.github.com/mu88/PasswordTrainer/commit/f855ad2bc5269b7b250591efd77135d385f054e0))
* **deps:** update all dependencies ([670e3ab](https://www.github.com/mu88/PasswordTrainer/commit/670e3abfc5eadd378e766bffb4d26c95a5e38a45))
* **deps:** update all dependencies ([56b5ba6](https://www.github.com/mu88/PasswordTrainer/commit/56b5ba60112bd75b19509d012d4245d01e2c066b))
* **deps:** update all dependencies ([bc44742](https://www.github.com/mu88/PasswordTrainer/commit/bc4474299a82b3c4862cb83528c56342ba6596f4))
* **deps:** update dependency mu88.shared to 8.1.0 [security] ([2598c24](https://www.github.com/mu88/PasswordTrainer/commit/2598c24cbc4bada6f3b36628fbe0c1469ec5c359))
* **deps:** update mu88.Shared to version 6.0.0 for latest OTEL fixes ([1c68e78](https://www.github.com/mu88/PasswordTrainer/commit/1c68e78ca53f7f5a393ac4915914ca398b332595))
* **deps:** update mu88.Shared to version 7.0.0 for latest OTEL fixes ([414d75f](https://www.github.com/mu88/PasswordTrainer/commit/414d75f8740901bba251b5d2f1af1a4928e8a6ce))
* **deps:** upgrade mu88.Shared for latest OTel / health check updates ([84b6be1](https://www.github.com/mu88/PasswordTrainer/commit/84b6be1805fc87a634ec21a8880744f0b3941ced))
* **dev:** fix broken DotSettings links ([ad0acaf](https://www.github.com/mu88/PasswordTrainer/commit/ad0acafa5d227126e53c84b5e0986d01e200d008))

<a name="1.2.0"></a>
## [1.2.0](https://www.github.com/mu88/PasswordTrainer/releases/tag/1.2.0) (2026-01-23)

### ✨ Features

* add configurable PathBase for custom endpoint paths ([8307fb2](https://www.github.com/mu88/PasswordTrainer/commit/8307fb2d8f5613b342c5e03c603564393766cf51))

<a name="1.1.0"></a>
## [1.1.0](https://www.github.com/mu88/PasswordTrainer/releases/tag/1.1.0) (2026-01-22)

### ✨ Features

* improve security by masking passwords upon entering and avoid passing passwords around as strings ([754b468](https://www.github.com/mu88/PasswordTrainer/commit/754b4682f4e05f9c959ad99a520add55b73b08bb))
* validate check request ([96953f8](https://www.github.com/mu88/PasswordTrainer/commit/96953f8309fab90cddc1566947ff8188b99dcc3c))

<a name="1.0.0"></a>
## [1.0.0](https://www.github.com/mu88/PasswordTrainer/releases/tag/v1.0.0) (2026-01-20)

### ✨ Features

* initialize app ([90ae09c](https://www.github.com/mu88/PasswordTrainer/commit/90ae09c9a1c0d6e071ffece5566c0729a47a3839))

