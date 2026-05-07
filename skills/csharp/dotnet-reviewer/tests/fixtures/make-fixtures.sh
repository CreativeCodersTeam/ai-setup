#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

reset_dir() {
  local d=$1
  rm -rf "$d"
  mkdir -p "$d"
}

git_init_at() {
  ( cd "$1" && git init -q -b main && \
    git config user.email "test@example.com" && \
    git config user.name "test" )
}

make_csproj() {
  local path=$1 tfm=$2
  cat > "$path" <<EOF
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>$tfm</TargetFramework>
  </PropertyGroup>
</Project>
EOF
}

make_global_json() {
  local path=$1 ver=$2
  cat > "$path" <<EOF
{
  "sdk": {
    "version": "$ver"
  }
}
EOF
}

# 1. repo-net10 — happy path
NET10="$ROOT/repo-net10"
reset_dir "$NET10"
git_init_at "$NET10"
make_global_json "$NET10/global.json" "10.0.100"
make_csproj "$NET10/App.csproj" "net10.0"
mkdir -p "$NET10/src" "$NET10/wwwroot/lib"
echo 'public class Hello { public string Name => "world"; }' > "$NET10/src/Hello.cs"
echo 'minified' > "$NET10/static.min.js"
echo 'vendor' > "$NET10/wwwroot/lib/vendor.js"
( cd "$NET10" && git add -A && git commit -q -m "initial" )
# Add an uncommitted change for uncommitted-mode tests
echo 'public class New { }' > "$NET10/src/New.cs"

# 2. repo-net8 — pre-10 SDK
NET8="$ROOT/repo-net8"
reset_dir "$NET8"
git_init_at "$NET8"
make_global_json "$NET8/global.json" "8.0.100"
make_csproj "$NET8/App.csproj" "net8.0"
( cd "$NET8" && git add -A && git commit -q -m "initial" )

# 3. repo-no-git — not a git repo
NOGIT="$ROOT/repo-no-git"
reset_dir "$NOGIT"
make_csproj "$NOGIT/App.csproj" "net10.0"

# 4. repo-malformed-csproj
MALF="$ROOT/repo-malformed-csproj"
reset_dir "$MALF"
git_init_at "$MALF"
cat > "$MALF/App.csproj" <<'EOF'
<Project Sdk="Microsoft.NET.Sdk"
  <PropertyGroup>
    <TargetFramework>net10.0
EOF
( cd "$MALF" && git add -A && git commit -q -m "broken" )

# 5. repo-large-diff — >2000 LOC, >50 files
LARGE="$ROOT/repo-large-diff"
reset_dir "$LARGE"
git_init_at "$LARGE"
make_global_json "$LARGE/global.json" "10.0.100"
make_csproj "$LARGE/App.csproj" "net10.0"
mkdir -p "$LARGE/src"
echo 'placeholder' > "$LARGE/src/seed.cs"
( cd "$LARGE" && git add -A && git commit -q -m "initial" )
# Generate 60 files with ~50 lines each → ~3000 LOC uncommitted
for i in $(seq 1 60); do
  {
    printf 'public class C%d {\n' "$i"
    for j in $(seq 1 48); do printf '    public int P%d => %d;\n' "$j" "$j"; done
    printf '}\n'
  } > "$LARGE/src/C${i}.cs"
done

# 6. repo-empty-diff — clean working tree
EMPTY="$ROOT/repo-empty-diff"
reset_dir "$EMPTY"
git_init_at "$EMPTY"
make_global_json "$EMPTY/global.json" "10.0.100"
make_csproj "$EMPTY/App.csproj" "net10.0"
echo 'public class Empty { }' > "$EMPTY/Empty.cs"
( cd "$EMPTY" && git add -A && git commit -q -m "initial" )

echo "Fixtures built under $ROOT"
