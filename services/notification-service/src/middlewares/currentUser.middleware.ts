import {NextFunction, Request, Response} from 'express';

export interface CurrentUser {
  userId: string | null;
  isSeller: boolean;
}

export function currentUserMiddleware(req: Request, _res: Response, next: NextFunction): void {
  const userIdHeader = req.header('X-User-Id');
  const rolesHeader = req.header('X-User-Roles');

  req.currentUser = {
    userId: userIdHeader && isUuid(userIdHeader) ? userIdHeader : null,
    isSeller: rolesHeader?.split(',').includes('seller') ?? false,
  };

  next();
}

function isUuid(value: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
}
