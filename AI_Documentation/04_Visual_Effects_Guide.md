# Visual Effects Guide (WPF)

## Active Task Glow Effect

Uses `DropShadowEffect` with animated `Opacity` and `BlurRadius`:

```xml
<Style x:Key="ActiveTaskStyle" TargetType="Border">
  <Setter Property="Effect">
    <Setter.Value>
      <DropShadowEffect ShadowDepth="0" Color="#6366F1"
                        BlurRadius="15" Opacity="0"/>
    </Setter.Value>
  </Setter>
  <Style.Triggers>
    <DataTrigger Binding="{Binding IsActive}" Value="True">
      <DataTrigger.EnterActions>
        <BeginStoryboard>
          <Storyboard RepeatBehavior="Forever" AutoReverse="True">
            <DoubleAnimation
              Storyboard.TargetProperty="(Effect).(DropShadowEffect.Opacity)"
              From="0.4" To="0.9" Duration="0:0:1"/>
            <DoubleAnimation
              Storyboard.TargetProperty="(Effect).(DropShadowEffect.BlurRadius)"
              From="10" To="25" Duration="0:0:1"/>
          </Storyboard>
        </BeginStoryboard>
      </DataTrigger.EnterActions>
    </DataTrigger>
  </Style.Triggers>
</Style>
```

## Task Enter Animation

```xml
<Storyboard x:Key="SlideInStoryboard">
  <DoubleAnimation Storyboard.TargetProperty="Opacity"
                   From="0" To="1" Duration="0:0:0.25"/>
  <DoubleAnimation Storyboard.TargetProperty="(RenderTransform).(TranslateTransform.Y)"
                   From="-10" To="0" Duration="0:0:0.25">
    <DoubleAnimation.EasingFunction>
      <CubicEase EasingMode="EaseOut"/>
    </DoubleAnimation.EasingFunction>
  </DoubleAnimation>
</Storyboard>
```

## Circular Timer Progress

WPF `Arc` via `Path` with `ArcSegment`, animated `stroke-dashoffset` equivalent:

```xml
<Path Stroke="#6366F1" StrokeThickness="6" StrokeStartLineCap="Round" StrokeEndLineCap="Round">
  <Path.Data>
    <PathGeometry>
      <PathFigure StartPoint="{Binding ArcStart}">
        <ArcSegment Point="{Binding ArcEnd}"
                    Size="54,54"
                    IsLargeArc="{Binding IsLargeArc}"
                    SweepDirection="Clockwise"/>
      </PathFigure>
    </PathGeometry>
  </Path.Data>
</Path>
```

Progress is driven by the ViewModel computing arc start/end points based on remaining time.

## Widget Background (Glassmorphism-like)

```xml
<Border Background="#E60F0F19" CornerRadius="16"
        BorderBrush="#10FFFFFF" BorderThickness="1">
  <!-- Content -->
</Border>
```

## Priority Color Indicators

```xml
<SolidColorBrush x:Key="PriorityLow" Color="#22C55E"/>
<SolidColorBrush x:Key="PriorityMedium" Color="#EAB308"/>
<SolidColorBrush x:Key="PriorityHigh" Color="#EF4444"/>
```

## Performance Notes

- WPF animations run on the composition thread (GPU-accelerated)
- `DropShadowEffect` is bitmap-cached automatically
- Use `CacheMode="BitmapCache"` on animated elements for extra smoothness
- `DispatcherTimer` at 1s interval for countdown — negligible CPU
