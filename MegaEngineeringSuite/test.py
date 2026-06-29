import math

def normalize_angle(angle):
    angle = angle % 360
    if angle < 0: angle += 360
    return angle

tubeRadius = 9.525
cutY = -120.0
tubeCenterX = -50.0
tubeCenterY = -120.0

rel_y = (cutY - tubeCenterY) / tubeRadius
rel_y = max(-1.0, min(1.0, rel_y))
inter_ang = math.asin(rel_y) * 180.0 / math.pi

startAngle = normalize_angle(inter_ang)
endAngle = normalize_angle(180.0 - inter_ang)

sweep = endAngle - startAngle
if sweep < 0: sweep += 360

print(f'Center: ({tubeCenterX}, {tubeCenterY})')
print(f'Radius: {tubeRadius}')
print(f'StartAngle: {startAngle}')
print(f'EndAngle: {endAngle}')
print(f'SweepAngle: {sweep}')

startRad = startAngle * math.pi / 180.0
endRad = endAngle * math.pi / 180.0

print('\n--- AutoLISP / entmake block ---')
print('    (command "-LAYER" "M" "TUBE_HOLES" "C" "1" "" "L" "CONTINUOUS" "" "")')
print(f'    (setq cen (list (+ (car pt) {tubeCenterX:.4f}) (+ (cadr pt) {tubeCenterY:.4f})))')
print('    (entmake (list \'(0 . "ARC")')
print('                   \'(8 . "TUBE_HOLES")')
print('                   (cons 10 cen)')
print(f'                   (cons 40 {tubeRadius:.4f})')
print(f'                   (cons 50 {startRad:.4f})')
print(f'                   (cons 51 {endRad:.4f})))')
