clc;
clear;

data = xlsread("LY-光学畸变0606.xlsx");
r_0D_r = data(:, 2);
R_0D_r = data(:, 3);
r_0D_g = data(:, 5);
R_0D_g = data(:, 6);
r_0D_b = data(:, 8);
R_0D_b = data(:, 9);

parms_r = fit(R_0D_r, r_0D_r);
fitted_function = @(x) parms_r(1) * x.^3 + parms_r(2) * x.^2 + parms_r(3) * x;
plot(R_0D_r, r_0D_r, 'ro', R_0D_r, fitted_function(R_0D_r), 'b-');
parms_g = fit(R_0D_g, r_0D_g);
hold on;
plot(R_0D_g, r_0D_g, 'ro', R_0D_g, fitted_function(R_0D_g), 'b-');
parms_b = fit(R_0D_b, r_0D_b);
hold on;
plot(R_0D_b, r_0D_b, 'ro', R_0D_b, fitted_function(R_0D_b), 'b-');

function params = fit(x_data, y_data)
    model = @(params, x) params(1) * x.^3 + params(2) * x.^2 + params(3) * x;
    options = optimoptions('lsqcurvefit', 'Display', 'off', 'MaxFunEvals', 10000);
    params = lsqcurvefit(model, [1, 1, 1], x_data, y_data, [], [], options);
end