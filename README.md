# ⭐ Star Defence ⭐

## 1. 프로젝트 개요

**Star Defence**는 정해진 길을 따라 몰려오는 적들을 막기 위해 영웅을 배치하고 성장시켜 나가는 타워 디펜스 게임입니다. 플레이어는 재화를 사용하여 영웅을 소환 및 승급시키고, 파괴된 타일을 수리하거나 특수 타일의 효과를 활용하는 등 다양한 전략적 선택을 통해 승리해야 합니다.

## 2. 핵심 시스템

프로젝트의 주요 기능과 이를 담당하는 핵심 스크립트 및 동작 방식은 다음과 같습니다.

### 2.1. 전투 루프

게임의 가장 기본적인 전투 사이클입니다.

#### 가. 적 생성 및 이동
- **담당:** `WaveManager.cs`, `Enemy.cs`, `Pathfinding.cs`
- **동작:**
  1. `WaveManager`가 `WaveData` (ScriptableObject)에 정의된 순서에 따라 적(Enemy)을 스폰합니다.
  2. 생성된 `Enemy`는 `GridManager`로부터 받은 길찾기 데이터를 `Pathfinding`에 요청하여 스폰 지점(`S`)부터 지휘관이 있는 목표 지점(`H`)까지 이동합니다.

#### 나. 영웅 공격
- **담당:** `Hero.cs` (추상 클래스) 및 하위 영웅 클래스들
- **동작:**
  1. 각 `Hero`는 자신의 `attackRange`(공격 사거리) 내에 있는 적을 주기적으로 탐색(`ScanForEnemiesCoroutine`)하여 가장 가까운 적을 `currentTarget`으로 설정합니다.
  2. `Update()` 메서드에서 `attackTimer`가 `CurrentAttackInterval`(버프가 적용된 최종 공격 속도) 값에 도달하면 `Attack()` 추상 메서드를 호출합니다.
  3. `Attack()`의 실제 공격 로직(근접, 원거리 등)은 각 영웅 타입의 하위 클래스에서 구현됩니다.

#### 다. 적 체력 감소 및 사망 처리
- **담당:** `Projectile.cs`, `Enemy.cs`, `GameManager.cs`
- **동작:**
  1. 원거리 영웅의 경우 `Attack()` 시 `Projectile`(투사체)을 생성하여 적에게 날려보냅니다.
  2. 투사체가 적에게 닿거나 근접 영웅의 공격이 발생하면 적의 `TakeDamage()` 메서드가 호출되어 체력이 감소합니다.
  3. 적의 체력이 0 이하가 되면 해당 적은 `Enemy.OnEnemyDestroyed` 이벤트를 발생시키고 풀(Pool)로 반환됩니다.
  4. `GameManager`는 이 이벤트를 구독하여(`HandleEnemyDestroyed`), 적의 `goldReward` 만큼 골드를 획득합니다.

---

### 2.2. 웨이브 및 게임 상태

게임의 시작, 진행, 종료를 관리합니다.

- **담당:** `GameManager.cs`, `WaveManager.cs`, `Commander.cs`
- **`GameStatus` Enum:** `Ready` → `Build` → `Wave` → `GameOver`/`Victory`
- **동작:**
  - **승리 판정:** `WaveManager`가 마지막 웨이브까지 모두 성공적으로 막아냈을 때 `GameManager.GameVictory()`를 호출하여 `Victory` 상태로 전환하고 승리 UI를 띄웁니다.
  - **패배 판정:** 적이 목표 지점에 도달하여 `Commander`의 체력을 모두 소진시키면 `Commander`가 `GameManager.GameOver()`를 호출하여 `GameOver` 상태로 전환하고 게임 오버 UI를 띄웁니다.

---

### 2.3. 영웅 시스템

영웅의 소환, 배치, 성장을 관리합니다.

- **담당:** `GameManager.cs`, `Hero.cs`, `HeroDataSO.cs`, `PlaceHeroConfirmUI.cs`
- **동작:**
  - **소환:**
    1. 플레이어가 배치 가능한 타일(`B`)을 클릭하면 `GameManager.HandleTileClicked`가 호출됩니다.
    2. `PlaceHeroConfirmUI`가 나타나고 확인 시 `GameManager.ConfirmPlaceHero()`가 실행됩니다.
    3. `tier1Heroes` 목록에서 무작위 영웅이 선택되고 `placementCost`만큼 **골드**를 소모하여 타일에 배치됩니다.
  - **승급 (동일 등급 합성):**
    1. 타일에 배치된 영웅을 클릭하면 `GameManager.TryUpgradeHero()`가 호출됩니다.
    2. 필드 위에 같은 종류의 다른 영웅(`mergePartner`)이 있는지 탐색합니다.
    3. 대상이 있으면 `PlaceHeroConfirmUI`를 통해 확인을 받고, `GameManager.ConfirmUpgradeHero()`가 실행됩니다.
    4. `upgradeCost`만큼 **미네랄**을 소모하고 두 하위 등급 영웅은 풀로 반환되며 `HeroDataSO`에 정의된 `nextTierHero`가 그 자리에 생성됩니다.

---

### 2.4. 탐사정 및 타일 시스템

전략성을 더하는 추가 기능들입니다.

- **담당:** `ProbeManager.cs`, `Tile.cs`, `GridManager.cs`
- **동작:**
  - **탐사정 (Probe):**
    1. UI 버튼을 통해 `GameManager.TryPurchaseProbe()`를 호출합니다.
    2. **골드**를 소모하여 `ProbeManager`가 탐사정을 생성합니다.
    3. 생성된 탐사정은 자동으로 미네랄을 채취하여 자원을 수급합니다. (상세 로직은 `Probe.cs`에 구현)
  - **타일 수리 (Tile Repair):**
    1. 파괴된 타일(`F`)을 클릭하면 수리 확인 UI가 나타납니다.
    2. `GameManager.ConfirmRepairTile()`이 호출되어 수리 횟수에 따라 증가하는 `repairCost`만큼 **미네랄**을 소모하고 타일을 배치 가능한 `B` 상태로 되돌립니다.
  - **강화 타일 (Buff Tile):**
    1. `GridManager`가 게임 시작 시 `GenerateBuffTiles()`를 호출합니다.
    2. 배치 가능한(`B`) 타일 중 `numberOfBuffTiles` 개수만큼 무작위로 선택됩니다.
    3. `possibleBuffs` 목록의 `BuffData`가 타일의 `CurrentBuff`로 할당되고 타일은 지정된 색상으로 빛납니다.
    4. 영웅이 해당 타일에 배치되면 `tile.SetHero()`를 통해 버프가 적용되어 `CurrentAttackInterval` 같은 능력치가 실시간으로 변경됩니다.

## 3. 향후 개발 계획

- **영웅 초월:** 최고 등급에 도달한 영웅을 한 번 더 강화하는 시스템.
- **현상금:** 특정 적 유닛에 추가 보상 부여
- **강화:** 게임 중 얻는 재화를 통해 영웅, 지휘관의 기본 능력치를 영구적으로 업그레이드하는 시스템.

---

## 4. 프로젝트 구조

`Assets` 폴더는 게임의 모든 리소스와 코드를 담고 있으며 기능별로 다음과 같이 구성되어 있습니다.

```
Assets
├── 📂 Artwork              # 게임에 사용되는 2D 이미지 리소스 (스프라이트, 배경, UI)
├── 📂 Resources             # Resources.Load()를 통해 동적으로 로드되는 애셋
│   ├── 📂 Data              # SpriteDatabase 등 데이터 애셋
│   ├── 📂 MapData           # 맵 생성을 위한 .csv 파일
│   └── 📂 Prefabs           # 게임에 등장하는 모든 오브젝트의 원본 (프리팹)
│       ├── Creatures      # 영웅, 적, 지휘관, 탐사정 프리팹
│       ├── Projectiles    # 투사체 프리팹
│       └── UI             # UI 요소 및 팝업창 프리팹
├── 📂 Scenes                # 게임 씬(.unity 파일)
├── 📂 ScriptableObjects     # 게임의 핵심 데이터를 담고 있는 스크립터블 오브젝트 애셋
│   ├── Buffs              # 버프 데이터 (공격속도 증가 등)
│   ├── Commander          # 지휘관 데이터
│   ├── Enemy              # 적 데이터
│   ├── Hero               # 영웅 등급별 데이터
│   ├── Probe              # 탐사정 데이터
│   └── Wave               # 웨이브 구성 데이터
├── 📂 Scripts               # 모든 C# 스크립트 소스 코드
│   ├── 📂 Common            # Singleton, Constants 등 공용 스크립트
│   ├── 📂 Creatures         # 영웅, 적 등 게임 유닛의 로직
│   ├── 📂 Data              # 데이터 관리 관련 스크립트
│   ├── 📂 Managers          # 게임의 핵심 동작을 관리하는 모든 매니저 클래스
│   ├── 📂 Stages            # Tile, WaveDataSO 등 스테이지 구성 관련 로직
│   └── 📂 UI                # UI 상호작용 및 표시 관련 로직
└── 📂 TextMesh Pro          # 텍스트 렌더링을 위한 TextMesh Pro 에셋
```