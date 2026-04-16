# Visual Effects Guide (WPF)

## Active Task State

Active tasks use an accent background and border instead of an animated blur. This keeps the current focus obvious without adding a costly `DropShadowEffect` to every task row.

```xml
<DataTrigger Binding="{Binding IsActive}" Value="True">
  <Setter TargetName="TaskBorder" Property="Background" Value="{StaticResource AccentSoftBrush}"/>
  <Setter TargetName="TaskBorder" Property="BorderBrush" Value="{StaticResource AccentBrush}"/>
</DataTrigger>
```

## Task Enter Animation

New task rows still use a short opacity and translate animation. This is cheap, helps the list feel responsive, and avoids constant animation during normal work.

```xml
<Storyboard>
  <DoubleAnimation Storyboard.TargetProperty="Opacity"
                   From="0" To="1" Duration="0:0:0.18"/>
  <DoubleAnimation Storyboard.TargetProperty="(RenderTransform).(TranslateTransform.Y)"
                   From="-6" To="0" Duration="0:0:0.18">
    <DoubleAnimation.EasingFunction>
      <CubicEase EasingMode="EaseOut"/>
    </DoubleAnimation.EasingFunction>
  </DoubleAnimation>
</Storyboard>
```

## Circular Timer Progress

The timer uses a `Path` whose geometry is driven by `ArcEndAngle`. The arc brush is named so code-behind can animate one brush color on phase changes.

```xml
<Path StrokeThickness="7" StrokeStartLineCap="Round" StrokeEndLineCap="Round">
  <Path.Stroke>
    <SolidColorBrush x:Name="ArcBrush" Color="#9BA3AF"/>
  </Path.Stroke>
  <Path.Data>
    <Binding Path="ArcEndAngle" Converter="{StaticResource AngleToArc}"/>
  </Path.Data>
</Path>
```

## Widget Surface

The shell uses a compact charcoal surface, an 8px radius, and a single outer shadow. Repeated inner panels use the `PanelCard` style from `Theme.xaml`.

```xml
<Border Background="{StaticResource BgPrimaryBrush}"
        CornerRadius="8"
        BorderBrush="{StaticResource WidgetBorderBrush}"
        BorderThickness="1">
  <!-- Content -->
</Border>
```

## Priority Color Indicators

Priority brushes are cached and frozen in `PriorityToColorConverter` to avoid allocating new brushes during binding refreshes.

- Low: green
- Medium: amber
- High: red

## Performance Notes

- Keep constant animation out of lists.
- Prefer color, opacity, and translate animations over blur effects.
- Use one animated timer brush instead of recreating arc visuals.
- `DispatcherTimer` at a 1s interval remains negligible for this widget.
