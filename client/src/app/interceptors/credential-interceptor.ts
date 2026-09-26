import { HttpInterceptorFn } from '@angular/common/http';

export const credentialInterceptor: HttpInterceptorFn = (req, next) => {
  return next(
    req.clone({
      withCredentials: true,
    }),
  );
};
