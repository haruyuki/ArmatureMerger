# Armature Merger

<img src="screenshot.png" height="500" alt="Screenshot of Armature Merger tool window">

## 日本語

このUnityエディタ拡張は、衣装のボーン階層をアバターのアーマチュアに簡単に統合するためのツールです。  
衣装があらかじめアバターに合わせて作られている場合に、ワンクリックでボーンのリネームとリパレントを行います。  
**VRM 0.X**（UniVRM）での使用を想定しています（VRM 1.Xは未テスト）。

### 機能
- アバターと衣装のオブジェクトをドラッグするだけでアーマチュアルートを自動検出
- 衣装のオブジェクト名からサフィックス（例：`_Cloth`）を自動検出（例：アバター名が「Avatar」、衣装名が「Avatar_Cloth」の場合、「Cloth」を抽出）
- サフィックスの手動入力も可能
- マージ時にボーン名にサフィックスを追加するかどうかを選択可能
- プレハブインスタンスの自動展開（必要に応じて）
- リパレント時にワールド位置を保持するオプション
- アンドゥ完全対応

---

## English

This Unity Editor extension helps you merge a clothing armature into an avatar's armature hierarchy with a single click.  
It is intended for cases where the clothing is already designed to fit the avatar and you just need to reparent the bones and optionally rename them.  
**Tested with VRM 0.X** (UniVRM). VRM 1.X has not been tested.

### Features
- Auto‑detects armature roots from avatar and clothing objects
- Auto‑detects a suffix from the clothing object name (e.g., if avatar is "Avatar" and clothing is "Avatar_Cloth", it extracts "Cloth")
- Manual suffix entry when auto‑detection fails
- Option to append the suffix to bone names during merge
- Automatically unpacks prefab instances if needed
- Option to preserve world position when reparenting
- Full Undo support

---

## Note on Alternatives / 代替ツールについて
より高度で非破壊的なアバター改変には、**NDMF**（Non-Destructive Modular Framework）や**Modular Avatar**などのツールがあります。  
本スクリプトは、衣装がすでにアバターにフィットする状態で、手軽にボーンを統合したい場合に適したシンプルなソリューションです。  
For more advanced, non‑destructive avatar workflows, consider using **NDMF** (Non-Destructive Modular Framework) or **Modular Avatar**.  
This script is a simple, single‑file solution for straightforward bone merging when the clothing is already avatar‑ready.


## Acknowledgements / 謝辞
This tool is inspired by the *Kisekae Bone Setup* created by **ureishi** in 2019.  
本ツールは、**ureishi**氏によって2019年に公開された*Kisekae Bone Setup*に触発されています。
