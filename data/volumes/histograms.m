I1 = imread("foot_16_16.png");
H1 = histcounts(I1);
[pks, idx] = findpeaks(H1);
plot(H1);
hold on;
scatter(idx,pks);