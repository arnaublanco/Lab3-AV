
I1 = imread("foot_16_16.png");
I2 = imread("teapot_16_16.png");
I3 = imread("brain_16_16.png");
I4 = imread("bonsai_16_16.png");

%%

figure;

H1 = histcounts(I1,'NumBins',256);
plot(H1);

%%
figure;

H2 = histcounts(I2,'NumBins',256);
plot(H2);

%%
figure;

H3 = histcounts(I3,'NumBins',256);
plot(H3);

%%
figure;

H4 = histcounts(I4,'NumBins',256);
plot(H4);