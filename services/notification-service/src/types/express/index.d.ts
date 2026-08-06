import {CurrentUser} from '../../middlewares/currentUser.middleware';

declare global {
  namespace Express {
    interface Request {
      currentUser: CurrentUser;
    }
  }
}

export {};
