# 패치 06 — DataGrid (그리드 컨트롤)

`ListView` + `GridView`가 읽기 전용 표라면 `DataGrid`는 편집 가능한 격자입니다. 셸과 같은 도면 톤으로 맞췄습니다.
패치 04(`Controls.xaml`)를 이미 적용한 상태를 전제로 합니다.

리포지토리에 아직 `DataGrid`를 쓰는 화면이 없어서 **스타일만** 넣습니다 — 나중에 격자를 붙이면 그대로 적용됩니다.

## 모양

- 헤더: 회색 판, 세로 구분선만, 정렬된 열은 accent 글자 + accent 화살표
- 행: 짝수 행 옅은 줄무늬, hover 옅은 틴트, 선택은 accent 틴트
- 행 머리(폭 26): 선택된 행만 accent 바로 채워져 어느 행인지 왼쪽 끝에서 바로 읽힘
- 편집 중인 셀만 accent 테두리 + 내부 여백을 줄여 입력칸처럼 보임
- 행 높이 34, 헤더 높이 34, 각진 모서리 · 1px 헤어라인

## 1. 파일 교체

| 파일 | 변경 |
| --- | --- |
| `src/UbisamBase.Core/Themes/Controls.xaml` | DataGrid 스타일 6종 추가 (파일 끝) |
| `src/UbisamBase.Core/Themes/Colors.Light.xaml` | 토큰 2개 추가 |
| `src/UbisamBase.Core/Themes/Colors.Dark.xaml` | 토큰 2개 추가 |

`code/UbisamBase.Core/Themes/` 아래 세 파일을 그대로 덮어쓰세요.

새 토큰:

| 키 | 라이트 | 다크 |
| --- | --- | --- |
| `Ubisam.Brush.Table.GridLine` | #DEDEE1 | #383D44 |
| `Ubisam.Brush.Table.AltRowBackground` | #F8F8F9 | #333840 |

## 2. 쓰는 법

암시적 스타일이라 `<DataGrid>`만 놓으면 적용됩니다. 열은 직접 정의하세요 (`AutoGenerateColumns`는 False가 기본):

```xml
<DataGrid ItemsSource="{Binding Measurements}"
          SelectedItem="{Binding SelectedMeasurement}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="시각" Width="96"
                            Binding="{Binding Timestamp, StringFormat=HH:mm:ss}"
                            IsReadOnly="True"/>
        <DataGridTextColumn Header="레시피" Width="*"
                            Binding="{Binding RecipeName}"
                            IsReadOnly="True"/>
        <DataGridTextColumn Header="측정값" Width="118"
                            Binding="{Binding Value, StringFormat=N1}"
                            CellStyle="{StaticResource Ubisam.Style.NumericCell}"/>
        <DataGridTextColumn Header="편차" Width="96"
                            Binding="{Binding Deviation, StringFormat=P1}"
                            CellStyle="{StaticResource Ubisam.Style.NumericCell}"
                            IsReadOnly="True"/>
    </DataGrid.Columns>
</DataGrid>
```

수치 열에는 `CellStyle="{StaticResource Ubisam.Style.NumericCell}"`을 주세요 — 우측 정렬됩니다.

편집을 아예 막을 화면은 `IsReadOnly="True"`를 `DataGrid`에 주면 됩니다.

## 3. 상태 열은 태그로

판정·레벨 같은 상태 열은 맨 텍스트로 두지 마세요 — 예외 행(재검, Warn)이 정상 행과 구분되지 않습니다. 태그 스타일을 쓰세요.

```xml
<DataGridTemplateColumn Header="판정" Width="96">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Border Style="{StaticResource Ubisam.Style.Tag.Accent}">
                <TextBlock Text="{Binding Verdict}" Style="{StaticResource Ubisam.Style.TagText.Accent}"/>
            </Border>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

세 종류입니다:

| 스타일 쌍 | 쓰는 곳 |
| --- | --- |
| `Ubisam.Style.Tag.Accent` + `TagText.Accent` | 정상·완료 (양품, Info) |
| `Ubisam.Style.Tag.Outline` + `TagText.Outline` | 주의·예외 (재검, Warn) — 외곽선만이라 눈에 걸립니다 |
| `Ubisam.Style.Tag` + `TagText` | 중립·대기 (—, Draft) |

상태값에 따라 스타일을 바꿔야 하면 `DataTrigger`로 `Border.Style`을 전환하거나, 값→스타일 컨버터를 하나 두세요.

새 토큰 4개(`Ubisam.Brush.Tag.*`)가 두 Colors 파일에 추가됐습니다.

## 4. 합계행

시안의 맨 아래 합계행은 `DataGrid`가 기본 제공하지 않습니다. 격자 아래에 `Grid` 한 줄을 따로 놓고 열 폭을 맞추거나, 필요하면 `DataGrid` 안에서 열 폭을 공유하는 방식으로 만들어야 합니다 — 어느 쪽으로 할지 알려주시면 그 부분만 따로 만들어 드립니다.

## 5. 확인

1. 헤더를 클릭해 정렬하면 그 열 글자가 accent로 바뀌고 화살표 방향이 뒤집히는지
2. 행을 선택하면 왼쪽 끝 행 머리가 accent 바로 채워지는지
3. 셀을 더블클릭해 편집에 들어가면 그 셀만 accent 테두리가 생기는지
4. 수치 열(측정값·편차)이 모두 우측 정렬인지 — 한쪽만 좌측이면 `CellStyle`이 빠진 것입니다
5. 판정 열의 재검 행이 양품 행과 한눈에 구분되는지
4. 짝수 행 줄무늬가 다크에서도 구분되는지(#333840)
5. 열 경계를 드래그해 폭 조절이 되는지 (그리퍼는 투명하지만 커서가 바뀝니다)

## 주의

- `DevExpress GridControl`이 아니라 **WPF 기본 `DataGrid`** 기준입니다. DevExpress를 쓰신다면 알려주세요 — 스타일 구조가 완전히 다릅니다.
- 줄무늬가 부담스러우면 `DataGrid`의 `AlternatingRowBackground`를 `Transparent`로 바꾸면 됩니다.
- 행을 더 밀집하려면 `RowHeight`를 34 → 28로, 셀 `Padding`을 `11,9` → `9,5`로 줄이세요.
