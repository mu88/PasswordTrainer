# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/versionize/versionize) for commit guidelines.

<a name="2.0.0"></a>
## [2.0.0](https://www.github.com/mu88/PasswordTrainer/releases/tag/2.0.0) (2026-11-26)

### ✨ Features

* migrate to .NET 11 with runtime async ([133b827](https://www.github.com/mu88/PasswordTrainer/commit/133b827f9f0db4603080d0cbc115479847a6d66d))

### 🐛 Bug Fixes

* resolve MA0206 code smell and close SecretInitializationWorker coverage gap ([092cc1f](https://www.github.com/mu88/PasswordTrainer/commit/092cc1ffac7eef12b4c629f3b7ac16dee09c48c7))
* update vulnerable .NET container runtime ([7119747](https://www.github.com/mu88/PasswordTrainer/commit/711974708589669e26ad97a1a300d28898b02c2a))
* **deps:** adopt shared testing publish worker fix ([3d92205](https://www.github.com/mu88/PasswordTrainer/commit/3d92205fe8373611fba2b4830e96048d772afea8))
* **security:** patch runtime and test dependency ([d7e8ac9](https://www.github.com/mu88/PasswordTrainer/commit/d7e8ac9009b4f17d59e3d246bf0756057d7e308c))
* **tests:** use valid SemVer for system test container image tag ([8b6fddc](https://www.github.com/mu88/PasswordTrainer/commit/8b6fddcb33785c0ea1421ff210facfa2eb842df5))
* **ux:** add Ctrl+C abort, DEL backspace, and loading indicator ([b2717ad](https://www.github.com/mu88/PasswordTrainer/commit/b2717ad9e849574775872204d9b23593ecb257cb))

### ♻️ Refactors

* **ci:** use centralized pin guard ([7756d60](https://www.github.com/mu88/PasswordTrainer/commit/7756d603f74da7e8f3b653bcadbab9aa7479765d))
* **tests:** adopt mu88.Shared.Testing for System test infrastructure ([c558eb6](https://www.github.com/mu88/PasswordTrainer/commit/c558eb6d8a149729cde87145425295aab8358214))

### 🔧 Chores

* **deps:** update actions/checkout action to v7 ([7c6a6ce](https://www.github.com/mu88/PasswordTrainer/commit/7c6a6ce69632d9f9185d85e97c913895e1fad0db))
* **deps:** update all .net ([640a038](https://www.github.com/mu88/PasswordTrainer/commit/640a038088d2b42fb3512eff1adcc0ede58b38e5))
* **deps:** update all .net ([7f83a4f](https://www.github.com/mu88/PasswordTrainer/commit/7f83a4f4ab97a765e52cbdabfac2400f780bf8f4))
* **deps:** update all .net ([5fa11b7](https://www.github.com/mu88/PasswordTrainer/commit/5fa11b7c85ad5c02e747438cad89f5deb2aa255f))
* **deps:** update all .net ([9a8d7ea](https://www.github.com/mu88/PasswordTrainer/commit/9a8d7ea8d88440f0890fc9d054198d682ee6942e))
* **deps:** update all .net ([07dbceb](https://www.github.com/mu88/PasswordTrainer/commit/07dbceb65fdec9d2e14d9e7f1435e29c12f4616e))
* **deps:** update all .net ([f087a6a](https://www.github.com/mu88/PasswordTrainer/commit/f087a6a3bdeef110399645d939d1b85802179ec9))
* **deps:** update all .net ([d016968](https://www.github.com/mu88/PasswordTrainer/commit/d01696891e654ec0ea8c63adfa2ea02b6ac519a1))
* **deps:** update all dependencies ([d59077c](https://www.github.com/mu88/PasswordTrainer/commit/d59077cbb7f5381a8a87ab99e6f1190848135ce3))
* **deps:** update all dependencies ([e13b885](https://www.github.com/mu88/PasswordTrainer/commit/e13b8854d953f1cb46545a4765fc95560fc7622c))
* **deps:** update all dependencies ([6f37b77](https://www.github.com/mu88/PasswordTrainer/commit/6f37b7747372c97de023122d5ccaa6838762a6bc))
* **deps:** update all dependencies ([7b2c261](https://www.github.com/mu88/PasswordTrainer/commit/7b2c2616c7bb2d88b28af29de8fb0cac27c9486a))
* **deps:** update all dependencies ([79c9593](https://www.github.com/mu88/PasswordTrainer/commit/79c959342b3854bf8a371f8a457125482537f103))
* **deps:** update all dependencies ([d8a453d](https://www.github.com/mu88/PasswordTrainer/commit/d8a453dac181e7ddb58add02c25e73355271e989))
* **deps:** update all dependencies ([55641eb](https://www.github.com/mu88/PasswordTrainer/commit/55641eb13560da21d32a66600882808a7c2ef2d3))
* **deps:** update all dependencies ([5815bdc](https://www.github.com/mu88/PasswordTrainer/commit/5815bdc9c0fb8d03d3be56cba6153e440ac2cfc1))
* **deps:** update all dependencies ([abb473d](https://www.github.com/mu88/PasswordTrainer/commit/abb473d0f0f04f34b0e98ad40cab877a67e959ae))
* **deps:** update all dependencies ([81adf41](https://www.github.com/mu88/PasswordTrainer/commit/81adf41e22e55222648a926db46018089413c4e7))
* **deps:** update all dependencies ([6ce64cd](https://www.github.com/mu88/PasswordTrainer/commit/6ce64cd44f02d320b02f6f7dab510215d8c2185e))
* **deps:** update all dependencies ([3307829](https://www.github.com/mu88/PasswordTrainer/commit/3307829a4c6cc65d88331753270dc70489e504e5))
* **deps:** update all dependencies ([51a4cbc](https://www.github.com/mu88/PasswordTrainer/commit/51a4cbc1e7248a0dd2d824e04bfdf22eca7f6d8e))
* **deps:** update all dependencies ([5f6365f](https://www.github.com/mu88/PasswordTrainer/commit/5f6365f2a8f2ad928b56149fc7e3519d02f2ce70))
* **deps:** update all dependencies ([75acfa5](https://www.github.com/mu88/PasswordTrainer/commit/75acfa517a88206c576e52a47fce85ecd71f0aef))
* **deps:** update all dependencies ([e79e7db](https://www.github.com/mu88/PasswordTrainer/commit/e79e7db11c053f0f9e68a6361bd560614b5abf61))
* **deps:** update all dependencies ([5e6ebb5](https://www.github.com/mu88/PasswordTrainer/commit/5e6ebb5ec61c4c47462fb7462aeba12e3a81cf1d))
* **deps:** update all dependencies to v18.8.0 ([db498e7](https://www.github.com/mu88/PasswordTrainer/commit/db498e74e9b8dd514e63c3a4de892270f818ef3d))
* **deps:** update dependency dotnet-coverage to v18.12.0 ([db5b9cb](https://www.github.com/mu88/PasswordTrainer/commit/db5b9cbb0c5c19d3d1d5fec232a2c70e595f57d5))
* **deps:** update dependency dotnet-coverage to v18.9.0 ([3ba35d3](https://www.github.com/mu88/PasswordTrainer/commit/3ba35d3f2f67beb5bf0843c1a1e4d20f95ac7b00))
* **deps:** update dependency dotnet-stryker to v4.15.0 ([9ed5614](https://www.github.com/mu88/PasswordTrainer/commit/9ed5614eef50fe8a0c6dd9d40a4f6c5427af792e))
* **deps:** update dependency dotnet-stryker to v4.16.0 ([f4e6c30](https://www.github.com/mu88/PasswordTrainer/commit/f4e6c3087593b2b0ca3b630e689880fe093d2a30))
* **deps:** update dependency microsoft.aspnetcore.mvc.testing to 10.0.6 ([53d5835](https://www.github.com/mu88/PasswordTrainer/commit/53d58354b7f40a557c6f032203a66c0893009ee7))
* **deps:** update dependency microsoft.net.test.sdk to 18.7.0 ([2d362f2](https://www.github.com/mu88/PasswordTrainer/commit/2d362f27f5254eb2700a47811a33976b535664e7))
* **deps:** update dependency mu88.shared to 8.6.0 ([516420c](https://www.github.com/mu88/PasswordTrainer/commit/516420cc8480abfa0d9e57b7588ef683865caef9))
* **deps:** update dependency testcontainers to 4.13.0 ([b341efc](https://www.github.com/mu88/PasswordTrainer/commit/b341efca8a129ceef3bddcc9e5e8d3af0f08bcdc))
* **deps:** update dependency versionize to v2.6.0 ([4bc846e](https://www.github.com/mu88/PasswordTrainer/commit/4bc846eb97d64d542c6f40d3696f4dcc6891d066))
* **deps:** update mu88/common digest to 03ce929 ([dda0165](https://www.github.com/mu88/PasswordTrainer/commit/dda0165269aa9705aab69bfd8d1d6c1bf8c661f0))
* **deps:** update mu88/common digest to 0d04e69 ([4693d65](https://www.github.com/mu88/PasswordTrainer/commit/4693d65e824950a66b2e5e1bf626779e85992ebb))
* **deps:** update mu88/common digest to 2edcdac ([a2f3c8f](https://www.github.com/mu88/PasswordTrainer/commit/a2f3c8f6973359911861f950ff15f4ac180f4cab))
* **deps:** update mu88/common digest to 309b9cb ([115d22c](https://www.github.com/mu88/PasswordTrainer/commit/115d22c0baa1a75b21cda296a1a647f92b83c9c3))
* **deps:** update mu88/common digest to 349db23 ([4020992](https://www.github.com/mu88/PasswordTrainer/commit/4020992e595eee1f8f37604e1f666d2e2428ac8b))
* **deps:** update mu88/common digest to 5d4855b ([64dca62](https://www.github.com/mu88/PasswordTrainer/commit/64dca6209051547a1a32a14bca6ba9c7868f5980))
* **deps:** update mu88/common digest to 64260e9 ([7cb5b89](https://www.github.com/mu88/PasswordTrainer/commit/7cb5b89754ebf3e494d6512638345c5ee5281242))
* **deps:** update mu88/common digest to 74c30dc ([aa09236](https://www.github.com/mu88/PasswordTrainer/commit/aa09236d6c48fffa304ebc1ea97bcb979a94607c))
* **deps:** update mu88/common digest to 88212c6 ([fcd2423](https://www.github.com/mu88/PasswordTrainer/commit/fcd2423c994105f2744bd94f2645eeb05be33aba))
* **deps:** update mu88/common digest to 9a7d93f ([b122943](https://www.github.com/mu88/PasswordTrainer/commit/b1229431c5758cabd1257aeb72ab929cde0d8916))
* **deps:** update mu88/common digest to b59f07c ([bd5833f](https://www.github.com/mu88/PasswordTrainer/commit/bd5833fe46cfd254e7af95cd070291618159a994))
* **deps:** update mu88/common digest to bf4949c ([c2e5621](https://www.github.com/mu88/PasswordTrainer/commit/c2e5621507f74dd7f95f1b52ed422c8a72a38e21))
* **deps:** update mu88/common digest to e3c88ef ([396ff6e](https://www.github.com/mu88/PasswordTrainer/commit/396ff6e81257604dc361d3add5ad80bc57bb39a0))
* **deps:** update mu88/common digest to fedc5a3 ([cabdcd4](https://www.github.com/mu88/PasswordTrainer/commit/cabdcd400b9bc7160af1f4ce07af4886d8b3ecb1))

### Breaking Changes

* migrate to .NET 11 with runtime async ([133b827](https://www.github.com/mu88/PasswordTrainer/commit/133b827f9f0db4603080d0cbc115479847a6d66d))

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

